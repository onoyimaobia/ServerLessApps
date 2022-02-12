using azureblob.Models;
using azureblob.Services;
using Microsoft.AspNetCore.Mvc;
namespace azureblob.Controllers
{
    /// <summary>
    ///  container
    /// </summary>
    public class ContainerController : Controller
    {
        private readonly IContainerService _containerservice;
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="containerservice"></param>
        public ContainerController(IContainerService containerservice)
        {
            _containerservice = containerservice;
        }
        public async Task<IActionResult> Index()
        {
            var containers = await _containerservice.GetAllContainers();
            return View(containers);
        }
        public async Task<IActionResult> Delete(string containerName)
        {
           await _containerservice.DeleteContainer(containerName);
            return RedirectToAction("Index");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public  IActionResult Create()
        {
            return View(new Container());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Create(Container container)
        {
            await _containerservice.CreateContainer(container.Name.ToLower().Trim());
            return RedirectToAction(nameof(Index));
        }
    }
}
