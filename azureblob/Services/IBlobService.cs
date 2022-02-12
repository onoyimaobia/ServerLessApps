using azureblob.Models;

namespace azureblob.Services
{
    /// <summary>
    ///  interface for  blob crud operation
    /// </summary>
    public interface IBlobService
    {
        /// <summary>
        /// upload a new blob
        /// </summary>
        /// <param name="name"></param>
        /// <param name="file"></param>
        /// <param name="containerName"></param>
        /// <param name="blob"></param>
        /// <returns></returns>
        Task<bool> UploadBlob(string name, IFormFile file, string containerName, Blob blob);
        /// <summary>
        /// delete a blob
        /// </summary>
        /// <param name="name"></param>
        /// <param name="containerName"></param>
        /// <returns>{bool}</returns>
        Task<bool> DeleteBlob(string name, string containerName);
        /// <summary>
        ///  get all blob in a particluar container 
        /// </summary>
        /// <returns></returns>
        Task<List<string>> GetAllBlobs(string containerName);
        /// <summary>
        /// get a single container
        /// </summary>
        /// <returns></returns>
        Task<string> GetBlob(string name, string containerName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="containerName"></param>
        /// <returns></returns>
        Task<List<Blob>> GetAllBlobsWithUri(string containerName);
    }
}
