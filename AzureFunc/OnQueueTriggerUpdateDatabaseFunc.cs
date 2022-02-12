using System;
using AzureFunc.Dbcontext;
using AzureFunc.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace AzureFunc
{
    public class OnQueueTriggerUpdateDatabaseFunc
    {
        private readonly AzureFuncDbContext _db;
        public OnQueueTriggerUpdateDatabaseFunc(AzureFuncDbContext db)
        {
            _db = db;
        }
        [FunctionName("OnQueueTriggerUpdateDatabaseFunc")]
        public void Run([QueueTrigger("SalesRequestInBound", Connection = "AzureWebJobsStorage")]SalesRequest myQueueItem, ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");

            myQueueItem.Status = "Submitted";
            _db.SalesRequests.Add(myQueueItem);
            _db.SaveChanges();
        }
    }
}
