namespace azureblob.Services
{
    /// <summary>
    /// interface used to perform crud operation on blob containers
    /// </summary>
    public interface IContainerService
    {
        /// <summary>
        /// create a new container
        /// </summary>
        /// <param name="containerName"></param>
        /// <returns></returns>
        Task CreateContainer(string containerName);
        /// <summary>
        /// delete a container by passing container name
        /// </summary>
        /// <param name="containerName"></param>
        /// <returns></returns>
        Task DeleteContainer(string containerName);
        /// <summary>
        ///  get all container created in the blob storage
        /// </summary>
        /// <returns></returns>
        Task<List<string>> GetAllContainers();
        /// <summary>
        /// get all container and the contents(blobs) created in the azure blob storage
        /// </summary>
        /// <returns></returns>
        Task<List<string>> GetAllContainersndBlobs();
    }
}
