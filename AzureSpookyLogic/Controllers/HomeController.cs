using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using AzureSpookyLogic.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;

namespace AzureSpookyLogic.Controllers
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
        [HttpPost]
        public async Task<IActionResult> IndexAsync(SpookyyRequest request, IFormFile formFile)
        {
            request.Id = Guid.NewGuid().ToString();
            var c = JsonConvert.SerializeObject(request);
            using var content = new StringContent(JsonConvert.SerializeObject(request),
                System.Text.Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _client.PostAsync("https://prod-10.centralus.logic.azure.com:443/workflows/d0f752c1e2534992993a619d770d3f6b/triggers/manual/paths/invoke?api-version=2016-10-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=zvxC4TEQPnpx4sPuxpCA5MQUF89y1uHflyY32RiWQ3M", content);
            if (formFile != null)
            {

                var fileName = request.Id + Path.GetExtension(formFile.FileName);
                BlobContainerClient blobContainerClient = _blobServiceClient.GetBlobContainerClient("logicappholder");
                BlobClient blobClient = blobContainerClient.GetBlobClient(fileName);
                var httpHeaders = new BlobHttpHeaders
                {
                    ContentType = formFile.ContentType,
                };
                await blobClient.UploadAsync(formFile.OpenReadStream(), httpHeaders);
            }
            return RedirectToAction("Index");
        }
        public IActionResult Privacy()
        {
            return View();
        }

       

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}