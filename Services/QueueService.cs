using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Threading.Tasks;
using ST10263027_CLDV6212_POE_2_.Models;

namespace ST10263027_CLDV6212_POE_2_.Services
{
    public class QueueService
    {
        private readonly QueueServiceClient _queueServiceClient;

        public QueueService(IConfiguration configuration)
        {
            _queueServiceClient = new QueueServiceClient(configuration["AzureStorage:ConnectionString"]);
        }

        public async Task SendMessageAsync(string queueName, string message)
        {
            var queueClient = _queueServiceClient.GetQueueClient(queueName);
            await queueClient.CreateIfNotExistsAsync();
            await queueClient.SendMessageAsync(message);
        }

        public async Task SendProductMessageAsync(string queueName, ProductProfile product)
        {
            var message = JsonConvert.SerializeObject(product);
            await SendMessageAsync(queueName, message);
        }

        public async Task<bool> QueueExistsAsync(string queueName)
        {
            var queueClient = _queueServiceClient.GetQueueClient(queueName);
            var response = await queueClient.ExistsAsync();
            return response.Value;
        }
    }
}