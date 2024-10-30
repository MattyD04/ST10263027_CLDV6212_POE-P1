using Azure;
using Azure.Data.Tables;
using System;

//code corrections by Claude AI
namespace ST10263027_CLDV6212_POE_2_.Models
{
    public class CustomerProfile : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string? ContractFilePath { get; set; }
        public string? ImageFilePath { get; set; }

        public CustomerProfile()
        {
            // Initialize with default values
            PartitionKey = "CustomerProfile";
            RowKey = Guid.NewGuid().ToString();
            Timestamp = DateTimeOffset.UtcNow;
            ETag = ETag.All;
            FirstName = string.Empty;
            SecondName = string.Empty;
            Email = string.Empty;
            PhoneNumber = string.Empty;
            ContractFilePath = string.Empty;
            ImageFilePath = string.Empty;
        }
    }
}