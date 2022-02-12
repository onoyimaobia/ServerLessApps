using azureblob.Models;
using azureblob.Services;
using Microsoft.AspNetCore.Mvc;

namespace azureblob.Controllers
{
    public class BlobController : Controller
    {
        private readonly IBlobService _blobService;
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="blobService"></param>
        public BlobController(IBlobService blobService)
        {
            _blobService = blobService;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="containerName"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Manage(string containerName)
        {
            var blobsObj = await _blobService.GetAllBlobs(containerName);
            return View(blobsObj);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="containerName"></param>
        /// <returns></returns>
        public async Task<IActionResult> ViewFile(string name, string containerName)
        {
            return Redirect(await _blobService.GetBlob(name, containerName));
        }
        /// <summary>
        /// 
        /// </summary> 
        /// <param name="name"></param>
        /// <param name="containerName"></param>
        /// <returns></returns>
        public async Task<IActionResult> DeleteFile(string name, string containerName)
        {
            await _blobService.DeleteBlob(name, containerName);
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="containerName"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult UploadBlob(string containerName)
        {
            return View();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> UploadBlob(string containerName,Blob blob, IFormFile file)
        {
            if(file == null || file.Length < 1) return View();
            var fileName = Path.GetFileNameWithoutExtension(file.FileName) + "_"+ Guid.NewGuid()+ Path.GetExtension(file.FileName);
            var result = await _blobService.UploadBlob(fileName, file, containerName, blob);
            if(result)
                return RedirectToAction("Index", "Container");
            return View();
        }
    }
}
