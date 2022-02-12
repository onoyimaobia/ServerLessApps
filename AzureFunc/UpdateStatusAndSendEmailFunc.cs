using System;
using System.Collections.Generic;
using System.Linq;
using AzureFunc.Dbcontext;
using AzureFunc.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace AzureFunc
{
    public class UpdateStatusAndSendEmailFunc
    {
        private readonly AzureFuncDbContext _db;
        public UpdateStatusAndSendEmailFunc(AzureFuncDbContext db)
        {
            _db = db;
        }
        [FunctionName("UpdateStatusAndSendEmailFunc")]
        public void Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            // get all sales request from  db that has image processed status.
            IEnumerable<SalesRequest> salesRequests = _db.SalesRequests.Where(x => x.Status == "Image Processed");
            foreach (SalesRequest salesRequest in salesRequests)
            {
                // update status
                salesRequest.Status = "Completed";
            }
            _db.UpdateRange(salesRequests);
            _db.SaveChangesAsync();

            // u can use sendgrid to send mail or use event grid to push to logic app and send mail using logic(gmail and yahoo can't work here).
        }
    }
}
