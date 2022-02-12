using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using AzureFunctionWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;

namespace AzureFunctionWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        static readonly HttpClient _client = new();
        private readonly BlobServiceClient _blobServiceClient;
        public HomeController(ILogger<HomeController> logger, BlobServiceClient blobClient)
        {
            _logger = logger;
            _blobServiceClient = blobClient;
        }

        public IActionResult Index()
        {
            return View();
        }
        //  http://localhost:7071/api/OnsalesRequestWriteToQueue

        [HttpPost]
        public async Task<IActionResult> Index(SalesRequest salesRequest, IFormFile formFile)
        {
            salesRequest.Id = Guid.NewGuid().ToString();
            using var content = new StringContent(JsonConvert.SerializeObject(salesRequest), 
                System.Text.Encoding.UTF8, "application/json");
            // call our function and pass the content
            HttpResponseMessage response = await _client.PostAsync("http://localhost:7071/api/OnsalesRequestWriteToQueueFunc", content);
            if (response.IsSuccessStatusCode)
            {
                string returnValue = response.Content.ReadAsStringAsync().Result;
                // upload to blob storage
                if(formFile != null)
                {
                  
                    var fileName = salesRequest.Id + Path.GetExtension(formFile.FileName);
                    BlobContainerClient blobContainerClient = _blobServiceClient.GetBlobContainerClient("flowers");
                    BlobClient blobClient = blobContainerClient.GetBlobClient(fileName);
                    var httpHeaders = new BlobHttpHeaders
                    {
                        ContentType = formFile.ContentType,
                    };
                    await blobClient.UploadAsync(formFile.OpenReadStream(), httpHeaders);
                }
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Privacy(FilesUpload filesUpload)
        {
            if (filesUpload != null && filesUpload.ZipFile.Length > 1)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(filesUpload.ZipFile.FileName);
                BlobContainerClient blobContainerClient = _blobServiceClient.GetBlobContainerClient("runtime-issue");
                BlobClient blobClient = blobContainerClient.GetBlobClient(fileName);
                var httpHeaders = new BlobHttpHeaders
                {
                    ContentType = filesUpload.ZipFile.ContentType,
                };
                await blobClient.UploadAsync(filesUpload.ZipFile.OpenReadStream(), httpHeaders);
            }
            return RedirectToAction("Privacy");
        }

       

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}