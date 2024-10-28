using Microsoft.Data.SqlClient;
using ST10263027_CLDV6212_POE_2_.Models;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace ST10263027_CLDV6212_POE_2_.Services
{
    public class ProductService
    {
        private readonly IConfiguration _configuration;

        public ProductService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task InsertProductAsync(ProductProfile profile)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var query = @"INSERT INTO productTable (productName, productPrice, productCategory, productAvailability)
                         VALUES (@productName, @productPrice, @productCategory, @productAvailability)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@productName", profile.ProductName);
                command.Parameters.AddWithValue("@productPrice", profile.ProductPrice);
                command.Parameters.AddWithValue("@productCategory", profile.ProductCategory);
                command.Parameters.AddWithValue("@productAvailability", profile.ProductAvailability);

                connection.Open();
                await command.ExecuteNonQueryAsync();
            }
        }
    }
}
