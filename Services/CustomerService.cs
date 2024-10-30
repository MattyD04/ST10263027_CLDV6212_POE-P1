using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ST10263027_CLDV6212_POE_2_.Models;
using System;
using System.Threading.Tasks;

//code corrections by Claude AI
namespace ST10263027_CLDV6212_POE_2_.Services
{
    public class CustomerService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<CustomerService> _logger;

        public CustomerService(IConfiguration configuration, ILogger<CustomerService> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> InsertCustomerAsync(CustomerProfile profile)
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(connectionString))
                {
                    _logger.LogError("Database connection string is missing");
                    return false;
                }

                // Validate profile data
                if (string.IsNullOrEmpty(profile.FirstName) ||
                    string.IsNullOrEmpty(profile.SecondName) ||
                    string.IsNullOrEmpty(profile.Email) ||
                    string.IsNullOrEmpty(profile.PhoneNumber))
                {
                    _logger.LogError("Required customer profile fields are missing");
                    return false;
                }

                // Updated query to match exact table schema
                var query = @"
                    INSERT INTO CustomerTable (
                        FirstName,
                        SecondName,
                        Email,
                        PhoneNumber
                    ) VALUES (
                        @FirstName,
                        @SecondName,
                        @Email,
                        @PhoneNumber
                    )";

                using (var connection = new SqlConnection(connectionString))
                {
                    using (var command = new SqlCommand(query, connection))
                    {
                        // Adding parameters with correct varchar(50) lengths
                        command.Parameters.Add("@FirstName", System.Data.SqlDbType.VarChar, 50).Value = profile.FirstName;
                        command.Parameters.Add("@SecondName", System.Data.SqlDbType.VarChar, 50).Value = profile.SecondName;
                        command.Parameters.Add("@Email", System.Data.SqlDbType.VarChar, 50).Value = profile.Email;
                        command.Parameters.Add("@PhoneNumber", System.Data.SqlDbType.VarChar, 50).Value = profile.PhoneNumber;

                        await connection.OpenAsync();
                        var result = await command.ExecuteNonQueryAsync();
                        return result > 0;
                    }
                }
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database error occurred while inserting customer: {Message}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while inserting customer: {Message}", ex.Message);
                throw;
            }
        }
    }
}