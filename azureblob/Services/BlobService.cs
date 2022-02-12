using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using azureblob.Models;

namespace azureblob.Services
{
    /// <summary>
    /// implementation of blob service
    /// </summary>
    public class BlobService : IBlobService
    {
        private readonly BlobServiceClient _blobServiceClient;
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="blobServiceClient"></param>
        public BlobService(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
        }

        /// <inheritdoc cref="IBlobService.DeleteBlob(string, string)"/>
        public async Task<bool> DeleteBlob(string name, string containerName)
        {
            BlobContainerClient blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = blobContainerClient.GetBlobClient(name);
            return await blobClient.DeleteIfExistsAsync();
        }

        /// <inheritdoc cref="IBlobService.GetAllBlobs(string)"/>
        public async Task<List<string>> GetAllBlobs(string containerName)
        {
            BlobContainerClient blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobs = blobContainerClient.GetBlobsAsync();
            var blobString = new List<string>();
            await foreach (var blob in blobs)
            {
                blobString.Add(blob.Name);
            }
            return blobString;
        }
        /// <inheritdoc cref="IBlobService.GetAllBlobsWithUri(string)"/>
        public async Task<List<Blob>> GetAllBlobsWithUri(string containerName)
        {
            BlobContainerClient blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobs = blobContainerClient.GetBlobsAsync();
            var blobList = new List<Blob>();
            // check if blob client can generate sas token at container level
            string sasContainerSignature = "";
            if (blobContainerClient.CanGenerateSasUri)
            {
                BlobSasBuilder sasBuilder = new()
                {
                    BlobContainerName = blobContainerClient.Name,
                    Resource = "c",
                    ExpiresOn = DateTime.UtcNow.AddHours(1),
                };
                sasBuilder.SetPermissions(BlobSasPermissions.Read);
               sasContainerSignature = blobContainerClient.GenerateSasUri(sasBuilder).AbsoluteUri.Split('?')[1].ToString();
            }

            await foreach (var item in blobs)
            {
                var blobClient = blobContainerClient.GetBlobClient(item.Name);
                Blob blob = new Blob()
                {
                    Uri = blobClient.Uri.AbsoluteUri + "?" + sasContainerSignature
                };
                //// check if blob client can generate sas token
                //if (blobClient.CanGenerateSasUri)
                //{
                //    BlobSasBuilder sasBuilder = new()
                //    {
                //        BlobContainerName = blobClient.GetParentBlobContainerClient().Name,
                //        BlobName = blobClient.Name,
                //        Resource = "b",
                //        ExpiresOn = DateTime.UtcNow.AddHours(1),
                //    };
                //    sasBuilder.SetPermissions(BlobSasPermissions.Read);
                //    blob.Uri = blobClient.GenerateSasUri(sasBuilder).AbsoluteUri;
                //}
                BlobProperties blobProperties = await blobClient.GetPropertiesAsync();
                if (blobProperties != null && blobProperties.Metadata.ContainsKey("title"))
                {
                   blob.Title = blobProperties.Metadata["title"];
                }
                if (blobProperties != null && blobProperties.Metadata.ContainsKey("comment"))
                {
                    blob.Comment = blobProperties.Metadata["comment"];
                }
                blobList.Add(blob);         
            }
            return blobList;
        }

        /// <inheritdoc cref="IBlobService.GetBlob(string, string)"/>
        public async Task<string> GetBlob(string name, string containerName)
        {
            BlobContainerClient blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = blobContainerClient.GetBlobClient(name);
            return blobClient.Uri.AbsoluteUri;
        }

        /// <inheritdoc cref="IBlobService.UploadBlob(string, IFormFile, string, Blob)"/>
        public async Task<bool> UploadBlob(string name, IFormFile file, string containerName, Blob blob)
        {
            BlobContainerClient blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = blobContainerClient.GetBlobClient(name);
            var httpHeaders = new BlobHttpHeaders()
            {
                ContentType = file.ContentType            
            };
            IDictionary<string, string> metadata = new Dictionary<string, string>();
            metadata.Add("title", blob.Title);
            metadata.Add("comment", blob.Comment);
            var result = await blobClient.UploadAsync(file.OpenReadStream(), httpHeaders, metadata);
            // remove metatadata
            //metadata.Remove("title");
            //await blobClient.SetMetadataAsync(metadata);


            if(result != null) return true;
            return false;
        }
    }
}
