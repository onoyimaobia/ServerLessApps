using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using AzureFunc.Model;

namespace AzureFunc
{
    public static class OnsalesRequestWriteToQueueFunc
    {
        [FunctionName("OnsalesRequestWriteToQueueFunc")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            [Queue("SalesRequestInBound", Connection = "AzureWebJobsStorage")] IAsyncCollector<SalesRequest> salesRequestQueue,
            ILogger log)
        {
            log.LogInformation("Sales request received by OnsalesRequestWriteToQueue function");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            SalesRequest data = JsonConvert.DeserializeObject<SalesRequest>(requestBody);

            // push data to a queue
            await salesRequestQueue.AddAsync(data);

            string responseMessage = "Sales Request has been received for ." + data.Name;

            return new OkObjectResult(responseMessage);
        }
    }
}
