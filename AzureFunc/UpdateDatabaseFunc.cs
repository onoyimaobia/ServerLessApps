using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AzureFunc.Dbcontext;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AzureFunc
{
    public class UpdateDatabaseFunc
    {
        private readonly AzureFuncDbContext _db;
        public UpdateDatabaseFunc(AzureFuncDbContext db)
        {
            _db = db;
        }
        [FunctionName("UpdateDatabaseFunc")]
        public async Task RunAsync([BlobTrigger("flowers-sm/{name}", Connection = "AzureWebJobsStorage")]Stream myBlob, string name, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
            var fileName = Path.GetFileNameWithoutExtension(name);
            var sales =  await _db.SalesRequests.Where(x => x.Id == fileName).FirstOrDefaultAsync();
           if(sales != null)
            {
                sales.Status = "Image Processed";
                _db.SalesRequests.Update(sales);
                _db.SaveChanges();
            }
                
        }
    }
}
