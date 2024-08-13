using Azure;
using Azure.Data.Tables;
using ST10263027_CLDV6212_POE_2_.Models;
using System.Threading.Tasks;

namespace ST10263027_CLDV6212_POE_2_.Services
{
    public class TableService
    {
        private readonly TableClient _tableClient;
        //hi
        public TableService(IConfiguration configuration)
        {
            var connectionString = configuration["AzureStorage: DefaultEndpointsProtocol=https;AccountName=matthewdstorage04;AccountKey=5sIl3ZRy3LnTC+B0vUAKkT+s7wktEhtiDEzhT0wOYfj0bnaQHgPZbLoyQR04qrZEEFn7Y+y+7mC++AStnAoZow==;EndpointSuffix=core.windows.net"];
            var serviceClient = new TableServiceClient(connectionString);
            _tableClient = serviceClient.GetTableClient("CustomerProfiles");
            _tableClient.CreateIfNotExists();
        }

        public async Task AddEntityAsync(CustomerProfile profile)
        {
            await _tableClient.AddEntityAsync(profile);
        }
    }
}
