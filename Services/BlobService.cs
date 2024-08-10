using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace ST10263027_CLDV6212_POE_2_.Services
{
    public class BlobService
    {
        private readonly BlobServiceClient _blobServiceClient;

        public BlobService(IConfiguration configuration)
        {
            _blobServiceClient = new BlobServiceClient(configuration["AzureStorage: DefaultEndpointsProtocol=https;AccountName=matthewdstorage04;AccountKey=5sIl3ZRy3LnTC+B0vUAKkT+s7wktEhtiDEzhT0wOYfj0bnaQHgPZbLoyQR04qrZEEFn7Y+y+7mC++AStnAoZow==;EndpointSuffix=core.windows.net"]);
        }

        public async Task UploadBlobAsync(string containerName, string blobName, Stream content)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync();
            var blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(content, true);
        }
    }

}
