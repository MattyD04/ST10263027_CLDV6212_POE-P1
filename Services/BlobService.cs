using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace ST10263027_CLDV6212_POE_2_.Services
{
    public class BlobService
    {
        private readonly string _connectionString;

        public BlobService(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Method to insert blob data into the SQL database
        public async Task<bool> InsertBlobAsync(byte[] imageData)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("INSERT INTO BlobTable (BlobImage) VALUES (@BlobImage)", connection))
                    {
                        command.Parameters.Add("@BlobImage", SqlDbType.VarBinary).Value = imageData;

                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        return rowsAffected > 0; // Return true if insert was successful
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as necessary
                Console.WriteLine($"Error inserting blob data: {ex.Message}");
                return false; // Return false if there was an error
            }
        }

        // Method to retrieve blob data from the SQL database (if needed)
        public async Task<byte[]> GetBlobAsync(int blobId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("SELECT BlobImage FROM BlobTable WHERE BlobID = @BlobID", connection))
                    {
                        command.Parameters.Add("@BlobID", SqlDbType.Int).Value = blobId;

                        // Execute the command and read the data
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return reader["BlobImage"] as byte[]; // Return the blob data
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as necessary
                Console.WriteLine($"Error retrieving blob data: {ex.Message}");
            }

            return null; // Return null if no data was found or if there was an error
        }

        // Method to delete blob data from the SQL database (if needed)
        public async Task<bool> DeleteBlobAsync(int blobId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("DELETE FROM BlobTable WHERE BlobID = @BlobID", connection))
                    {
                        command.Parameters.Add("@BlobID", SqlDbType.Int).Value = blobId;

                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        return rowsAffected > 0; // Return true if delete was successful
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as necessary
                Console.WriteLine($"Error deleting blob data: {ex.Message}");
                return false; // Return false if there was an error
            }
        }
    }
}
