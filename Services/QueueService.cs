using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;


namespace ST10263027_CLDV6212_POE_2_.Services
{
    public class QueueService
    {
        private readonly QueueServiceClient _queueServiceClient;

        public QueueService(IConfiguration configuration)
        {
            _queueServiceClient = new QueueServiceClient(configuration["AzureStorage: DefaultEndpointsProtocol=https;AccountName=matthewdstorage04;AccountKey=5sIl3ZRy3LnTC+B0vUAKkT+s7wktEhtiDEzhT0wOYfj0bnaQHgPZbLoyQR04qrZEEFn7Y+y+7mC++AStnAoZow==;EndpointSuffix=core.windows.net"]);
        }

        public async Task SendMessageAsync(string queueName, string message)
        {
            var queueClient = _queueServiceClient.GetQueueClient(queueName);
            await queueClient.CreateIfNotExistsAsync();
            await queueClient.SendMessageAsync(message);
        }
    }

}
