using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace azureblob.Services
{
    /// <summary>
    /// implemetation of i conatiner service. 
    /// This service implementation lets you perform crud
    /// operation in azure blob storage account by injecting blob client.
    /// </summary>
    public class ContainerService : IContainerService
    {
        private readonly BlobServiceClient _blobServiceClient;
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="blobServiceClient"></param>
        public ContainerService(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
        }

        /// <inheritdoc cref="IContainerService.CreateContainer(string)"/>
        public async Task CreateContainer(string containerName)
        {
            BlobContainerClient blobContainerClient= _blobServiceClient.GetBlobContainerClient(containerName);
            await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.BlobContainer);
        }

        /// <inheritdoc cref="IContainerService.DeleteContainer(string)"/>
        public async Task DeleteContainer(string containerName)
        {
            BlobContainerClient blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await blobContainerClient.DeleteIfExistsAsync();
        }

        /// <inheritdoc cref="IContainerService.GetAllContainers"/>
        public async Task<List<string>> GetAllContainers()
        {
           List<string> containerName = new();
            await foreach(BlobContainerItem blobContainerItem in _blobServiceClient.GetBlobContainersAsync())
            {
                containerName.Add(blobContainerItem.Name);
            }
            return containerName;
        }

        /// <inheritdoc cref="IContainerService.GetAllContainersndBlobs"/>
        public async Task<List<string>> GetAllContainersndBlobs()
        {
           List<string> containerAndBlobNames = new();
            containerAndBlobNames.Add("Account Name: "+ _blobServiceClient.AccountName);
            containerAndBlobNames.Add("-------------------------------------------------------");
            await foreach(BlobContainerItem blobContainerItem  in _blobServiceClient.GetBlobContainersAsync())
            {
                containerAndBlobNames.Add("--" + blobContainerItem.Name);
                BlobContainerClient blobContainer = _blobServiceClient.GetBlobContainerClient(blobContainerItem.Name);
                await foreach(BlobItem blobItem in blobContainer.GetBlobsAsync())
                {
                    //get metadata
                    var blobClient = blobContainer.GetBlobClient(blobItem.Name);
                    BlobProperties blobProperties = await blobClient.GetPropertiesAsync();
                    string blobToAdd = blobItem.Name;
                    if (blobProperties != null && blobProperties.Metadata.ContainsKey("title"))
                    {
                        blobToAdd += " "+ "(" + blobProperties.Metadata["title"] + ")";
                    }
                    containerAndBlobNames.Add("-------" + blobToAdd);
                }
                containerAndBlobNames.Add("-------------------------------------------------------");
            }
            return containerAndBlobNames;
        }
    }
}
