using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ST10263027_CLDV6212_POE_2_.Models;
using System;
using System.Threading.Tasks;

namespace ST10263027_CLDV6212_POE_2_.Services
{
    public class OrderService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<OrderService> _logger;

        public OrderService(IConfiguration configuration, ILogger<OrderService> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> InsertOrderAsync(OrderProfile order)
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(connectionString))
                {
                    _logger.LogError("Database connection string is missing");
                    return false;
                }

                // Validate order data
                if (string.IsNullOrEmpty(order.OrderNumber))
                {
                    _logger.LogError("Required order fields are missing");
                    return false;
                }

                // Updated query to match exact table schema
                var query = @"
                    INSERT INTO OrderTable (
                        OrderNumber
                    ) VALUES (
                        @OrderNumber
                    )";

                using (var connection = new SqlConnection(connectionString))
                {
                    using (var command = new SqlCommand(query, connection))
                    {
                        // Adding parameters with correct varchar(50) length
                        command.Parameters.Add("@OrderNumber", System.Data.SqlDbType.VarChar, 50).Value = order.OrderNumber;

                        await connection.OpenAsync();
                        var result = await command.ExecuteNonQueryAsync();
                        return result > 0; // Returns true if insertion is successful
                    }
                }
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database error occurred while inserting order: {Message}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while inserting order: {Message}", ex.Message);
                throw;
            }
        }
    }
}
