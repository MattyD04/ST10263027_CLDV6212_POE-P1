﻿using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;


namespace ST10263027_CLDV6212_POE_2_.Services
{
    public class FileService
    {
        private readonly ShareServiceClient _shareServiceClient;

        public FileService(IConfiguration configuration)
        {
            _shareServiceClient = new ShareServiceClient(configuration["AzureStorage: DefaultEndpointsProtocol=https;AccountName=matthewdstorage04;AccountKey=5sIl3ZRy3LnTC+B0vUAKkT+s7wktEhtiDEzhT0wOYfj0bnaQHgPZbLoyQR04qrZEEFn7Y+y+7mC++AStnAoZow==;EndpointSuffix=core.windows.net"]);
        }

        public async Task UploadFileAsync(string shareName, string fileName, Stream content)
        {
            var shareClient = _shareServiceClient.GetShareClient(shareName);
            await shareClient.CreateIfNotExistsAsync();
            var directoryClient = shareClient.GetRootDirectoryClient();
            var fileClient = directoryClient.GetFileClient(fileName);
            await fileClient.CreateAsync(content.Length);
            await fileClient.UploadAsync(content);
        }
    }

}