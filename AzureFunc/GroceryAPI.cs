using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using AzureFunc.Dbcontext;
using AzureFunc.Model;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;

namespace AzureFunc
{
    public  class GroceryAPI
    {
        private readonly AzureFuncDbContext _db;
        public GroceryAPI(AzureFuncDbContext db)
        {
            _db = db;
        }
        [FunctionName("CreateGrocery")]
        public  async Task<IActionResult> CreateGrocery(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "GroceryList")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
           

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            GroceryItem data = JsonConvert.DeserializeObject<GroceryItem>(requestBody);
           
            _db.GroceryItems.Add(data);
            await _db.SaveChangesAsync();

            

            return new OkObjectResult(data);
        }

        [FunctionName("CreateGroceryList")]
        public async Task<IActionResult> CreateGroceryList(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = "GroceryLists")] HttpRequest req,
           ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");


            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            List<GroceryItem> data = JsonConvert.DeserializeObject<List<GroceryItem>>(requestBody);
            await _db.GroceryItems.AddRangeAsync(data);
            await _db.SaveChangesAsync();



            return new OkObjectResult(data);
        }

        [FunctionName("GetGrocery")]
        public async Task<IActionResult> GetGrocery(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = "GroceryList")] HttpRequest req,
           ILogger log)
        {
            log.LogInformation("get all grocery");



            return new OkObjectResult(await _db.GroceryItems.ToListAsync());
        }

        [FunctionName("GetGroceryById")]
        public async Task<IActionResult> GetGroceryById(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = "GroceryList/{id}")] HttpRequest req,
           ILogger log, string id)
        {
            log.LogInformation("get grocery by id");

            var grocery = await _db.GroceryItems.FirstOrDefaultAsync(x => x.Id == id);
            if (grocery == null) return new NotFoundResult();

            return new OkObjectResult(grocery);
        }

        [FunctionName("DeleteGrocery")]
        public async Task<IActionResult> DeleteGrocery(
           [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "GroceryList/{id}")] HttpRequest req,
           ILogger log, string id)
        {
            log.LogInformation("get grocery by id");

            var grocery = await _db.GroceryItems.FirstOrDefaultAsync(x => x.Id == id);
            if (grocery == null) return new NotFoundResult();
            _db.GroceryItems.Remove(grocery);
            await _db.SaveChangesAsync();
            return new OkObjectResult("success");
        }

        [FunctionName("UpdateGrocery")]
        public async Task<IActionResult> UpdateGrocery(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = "GroceryList/{id}")] HttpRequest req,
           ILogger log, string id)
        {
            log.LogInformation("get grocery by id");

            var grocery = await _db.GroceryItems.FirstOrDefaultAsync(x => x.Id == id);
            if (grocery == null) return new NotFoundResult();
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            GroceryItem data = JsonConvert.DeserializeObject<GroceryItem>(requestBody);
            grocery.Name = data.Name;
            _db.GroceryItems.Update(grocery);
            await _db.SaveChangesAsync();

            return new OkObjectResult(grocery);
        }
    }
}
