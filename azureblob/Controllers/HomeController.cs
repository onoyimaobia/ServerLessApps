using azureblob.Models;
using azureblob.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace azureblob.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IContainerService _containerservice;
        private readonly IBlobService _blobService;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="containerservice"></param>
        /// <param name="blobService"></param>
        public HomeController(ILogger<HomeController> logger, IContainerService containerservice,
            IBlobService blobService)
        {
            _logger = logger;
            _containerservice = containerservice;
            _blobService = blobService;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _containerservice.GetAllContainersndBlobs());
        }

        public async Task<IActionResult> Images()
        {
            return View(await _blobService.GetAllBlobsWithUri("dotnet-images"));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}