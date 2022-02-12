using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AzureDurable.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail;

namespace AzureDurable
{
    public static class DurableFunc
    {
        [FunctionName("DurableFunc")]
        public static async Task<bool> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var isApproved = false;
            string meansOfApproval = Environment.GetEnvironmentVariable("Workflow:MeansOfApproval");
            ApprovalRequestMetadata approvalRequestMetadata = context.GetInput<ApprovalRequestMetadata>();
            approvalRequestMetadata.InstanceId = context.InstanceId;

            // Check whether the approval request is to be sent via Email or Slack based on App Settings
            if (meansOfApproval.Equals("email", StringComparison.OrdinalIgnoreCase))
            {
                await context.CallActivityAsync("SendApprovalRequestViaEmail", approvalRequestMetadata);
            }
            else
            {
                await context.CallActivityAsync("SendApprovalRequestViaSlack", approvalRequestMetadata);
            }

            // Wait for Response as an external event or a time out. 
            // The approver has a limit to approve otherwise the request will be rejected.
            using (var timeoutCts = new CancellationTokenSource())
            {
                int timeout;
                if (!int.TryParse(Environment.GetEnvironmentVariable("Workflow:Timeout"), out timeout))
                    timeout = 5;
                DateTime expiration = context.CurrentUtcDateTime.AddMinutes(timeout);
                Task timeoutTask = context.CreateTimer(expiration, timeoutCts.Token);

                // This event can come from a click on the Email sent via SendGrid or a selection on the message sent via Slack. 
                Task<bool> approvalResponse = context.WaitForExternalEvent<bool>("ReceiveApprovalResponse");
                Task winner = await Task.WhenAny(approvalResponse, timeoutTask);
                ApprovalResponseMetadata approvalResponseMetadata = new ApprovalResponseMetadata()
                {
                    ReferenceUrl = approvalRequestMetadata.ReferenceUrl
                };

                if (winner == approvalResponse)
                {
                    if (approvalResponse.Result)
                    {
                        approvalResponseMetadata.DestinationContainer = "approved";
                    }
                    else
                    {
                        approvalResponseMetadata.DestinationContainer = "rejected";
                    }
                }
                else
                {
                    approvalResponseMetadata.DestinationContainer = "rejected";
                }

                if (!timeoutTask.IsCompleted)
                {
                    // All pending timers must be completed or cancelled before the function exits.
                    timeoutCts.Cancel();
                }

                // Once the approval process has been finished, the Blob is to be moved to the corresponding container.
                await context.CallActivityAsync<string>("MoveBlob", approvalResponseMetadata);
                return isApproved;
            }

        }

        [FunctionName("MoveBlob")]
        public static void MoveBlob([ActivityTrigger] ApprovalResponseMetadata responseMetadata, ILogger log)
        {
            log.LogInformation($"Moving Blob {responseMetadata.ReferenceUrl} to {responseMetadata.DestinationContainer}");
            try
            {
                CloudStorageAccount account = CloudStorageAccount.Parse(System.Environment.GetEnvironmentVariable("Blob:StorageConnection", EnvironmentVariableTarget.Process));
                var client = account.CreateCloudBlobClient();
                var sourceBlob = client.GetBlobReferenceFromServerAsync(new Uri(responseMetadata.ReferenceUrl)).Result;
                var destinationContainer = client.GetContainerReference(responseMetadata.DestinationContainer);
                var destinationBlob = destinationContainer.GetBlobReference(sourceBlob.Name);
                destinationBlob.StartCopyAsync(sourceBlob.Uri);
                Task.Delay(TimeSpan.FromSeconds(15)).Wait();
                sourceBlob.DeleteAsync();
                log.LogInformation($"Blob '{responseMetadata.ReferenceUrl}' moved to container '{responseMetadata.DestinationContainer}'");
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
                throw;
            }
        }

        [FunctionName("SendApprovalRequestViaEmail")]
        public static  void SendApprovalRequestViaEmail(
            [ActivityTrigger] ApprovalRequestMetadata requestMetadata, [SendGrid] out SendGridMessage message,
            ILogger log)
        {
            message = new SendGridMessage();
            message.AddTo(Environment.GetEnvironmentVariable("SendGrid:To"));
            message.AddContent("text/html", string.Format(Environment.GetEnvironmentVariable("SendGrid:ApprovalEmailTemplate"), requestMetadata.Subject, requestMetadata.Requestor, requestMetadata.ReferenceUrl, requestMetadata.InstanceId, Environment.GetEnvironmentVariable("Function:BasePath")));
            message.SetFrom(Environment.GetEnvironmentVariable("SendGrid:From"));
            message.SetSubject(String.Format(Environment.GetEnvironmentVariable("SendGrid:SubjectTemplate"), requestMetadata.Subject, requestMetadata.Requestor));
            log.LogInformation($"Message '{message.Subject}' sent!");
        }

        [FunctionName("SendApprovalRequestViaSlack")]
        public static async Task<string> SendApprovalRequestViaSlackAsync(
            [ActivityTrigger] ApprovalRequestMetadata requestMetadata,
            ILogger log)
        {
            string approvalRequestUrl = Environment.GetEnvironmentVariable("Slack:ApprovalUrl", EnvironmentVariableTarget.Process);
            string approvalMessageTemplate = Environment.GetEnvironmentVariable("Slack:ApprovalMessageTemplate", EnvironmentVariableTarget.Process);
            Uri uri = new Uri(requestMetadata.ReferenceUrl);
            string approvalMessage = string.Format(approvalMessageTemplate, requestMetadata.ReferenceUrl, requestMetadata.ApprovalType, requestMetadata.InstanceId, requestMetadata.Requestor, requestMetadata.Subject);
            string resultContent;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(approvalRequestUrl);
                var content = new StringContent(approvalMessage, UnicodeEncoding.UTF8, "application/json");
                var result = await client.PostAsync(approvalRequestUrl, content);
                resultContent = await result.Content.ReadAsStringAsync();
                if (result.StatusCode != HttpStatusCode.OK)
                {
                    throw new HttpRequestException(resultContent);
                }
            }
            log.LogInformation($"Message regarding {requestMetadata.Subject} sent to Slack!");
            return resultContent;
        }

        /// <summary>
        /// Process approval responses via an HTTP GET with query params
        /// I'm using AuthorizationLevel.Anonymous just for demostration purposes, but you most probably want to authenticate and authorise the call. 
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("ProcessHttpGetApprovals")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, methods: "get", Route = "approval")] HttpRequestMessage req, [DurableClient] IDurableOrchestrationClient orchestrationClient, ILogger log)
        {
            log.LogInformation($"Received an Approval Respose");
            string name = req.RequestUri.ParseQueryString().GetValues("name")[0];
            string instanceId = req.RequestUri.ParseQueryString().GetValues("instanceid")[0];
            string response = req.RequestUri.ParseQueryString().GetValues("response")[0];
            log.LogInformation($"name: '{name}', instanceId: '{instanceId}', response: '{response}'");
            bool isApproved = false;
            var status = await orchestrationClient.GetStatusAsync(instanceId);
            log.LogInformation($"Orchestration status: {status}");
            if (status != null && (status.RuntimeStatus == OrchestrationRuntimeStatus.Running || status.RuntimeStatus == OrchestrationRuntimeStatus.Pending))
            {
                if (response.ToLower() == "approved")
                    isApproved = true;
                await orchestrationClient.RaiseEventAsync(instanceId, "ReceiveApprovalResponse", isApproved);
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("Thanks for your selection! :)") };
            }
            else
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent("Whoops! Something went wrong! :(") };
            }
        }

        /// <summary>
        /// Processes Slack Interactive Message Responses.
        /// Responses are received as 'application/x-www-form-urlencoded'
        /// Routes the response to the corresponding Durable Function orchestration instance 
        /// More information at https://api.slack.com/docs/message-buttons
        /// I'm using AuthorizationLevel.Anonymous just for demostration purposes, but you most probably want to authenticate and authorise the call. 
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("ProcessSlackApprovals")]
        public static async Task<HttpResponseMessage> ProcessSlackApprovals(
            [HttpTrigger(AuthorizationLevel.Anonymous, methods: "post", Route = "slackapproval")] HttpRequestMessage req, [DurableClient] IDurableOrchestrationClient orchestrationClient, ILogger log)
        {
            var formData = await req.Content.ReadAsFormDataAsync();
            string payload = formData.Get("payload");
            dynamic response = JsonConvert.DeserializeObject(payload);
            string callbackId = response.callback_id;
            string[] callbackIdParts = callbackId.Split('#');
            string approvalType = callbackIdParts[0];
            log.LogInformation($"Received a Slack Response with callbackid {callbackId}");

            string instanceId = callbackIdParts[1];
            string from = Uri.UnescapeDataString(callbackIdParts[2]);
            string name = callbackIdParts[3];
            bool isApproved = false;
            log.LogInformation($"instaceId:'{instanceId}', from:'{from}', name:'{name}', response:'{response.actions[0].value}'");
            var status = await orchestrationClient.GetStatusAsync(instanceId);
            log.LogInformation($"Orchestration status: '{status}'");
            if (status.RuntimeStatus == OrchestrationRuntimeStatus.Running || status.RuntimeStatus == OrchestrationRuntimeStatus.Pending)
            {
                string selection = response.actions[0].value;
                string emoji = "";
                if (selection == "Approve")
                {
                    isApproved = true;
                    emoji = ":rabbit:";
                }
                else
                {
                    emoji = ":rabbit2:";
                }
                await orchestrationClient.RaiseEventAsync(instanceId, "ReceiveApprovalResponse", isApproved);
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent($"Thanks for your selection! Your selection for *'{name}'* was *'{selection}'* {emoji}") };
            }
            else
            {
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent($"The approval request has expired! :crying_cat_face:") };
            }
        }
    }
}