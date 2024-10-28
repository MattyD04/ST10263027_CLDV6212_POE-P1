using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace ST10263027_CLDV6212_POE_2_.Services
{
    public class BlobService
    {
        private readonly IConfiguration _configuration;

        public BlobService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task InsertBlobAsync(byte[] imageData)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var query = @"INSERT INTO BlobTable (BlobImage) VALUES (@BlobImage)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@BlobImage", imageData);

                connection.Open();
                await command.ExecuteNonQueryAsync();
            }
        }
    }
}
