using System;
using System.IO;
using System.Threading.Tasks;
using AzureDurable.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace AzureDurable
{
    public class BlobContainerTrigger
    {
        [FunctionName("BlobContainerTrigger")]
        public async Task RunAsync([BlobTrigger("runtime-issue/{name}",
            Connection = "AzureWebJobsStorage")]Stream myBlob,
            string name,
             [DurableClient] IDurableOrchestrationClient  durableOrchestrationClient,
            ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
            // get your blob content, and desrialize if you need and pass it orchestrator instead of stream as below
            string blobStorageBasePath = Environment.GetEnvironmentVariable("Blob:StorageBasePath", EnvironmentVariableTarget.Process);
            string requestor = "";
            string subject = "";
            // If the blob name containes a '+' sign, it identifies the first part of the blob name as the requestor and the remaining as the subject. Otherwise, the requestor is unknown and the subject is the full blobname. 
            if (name.Contains("+"))
            {
                requestor = Uri.UnescapeDataString(name.Substring(0, name.LastIndexOf("+")));
                subject = name.Substring(name.LastIndexOf("+") + 1);
            }
            else
            {
                requestor = "unknown";
                subject = name;
            }

            ApprovalRequestMetadata requestMetadata = new ApprovalRequestMetadata()
            {
                ApprovalType = "FurryModel",
                ReferenceUrl = $"{blobStorageBasePath}requests/{name}",
                Subject = subject,
                Requestor = requestor
            };

            string instanceId =  await durableOrchestrationClient.StartNewAsync("DurableFunc", requestMetadata);
            log.LogInformation($"Durable Function Ochestration started: {instanceId}");
        }
    }
}
