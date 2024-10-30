using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ST10263027_CLDV6212_POE_2_.Models;
using ST10263027_CLDV6212_POE_2_.Services;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

//code corrections by Claude AI
namespace ST10263027_CLDV6212_POE_2_.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        private readonly CustomerService _customerService; // Inject CustomerService
        private readonly BlobService _blobService; // Inject BlobService

        public HomeController(IHttpClientFactory httpClientFactory, ILogger<HomeController> logger, IConfiguration configuration, CustomerService customerService, BlobService blobService)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _configuration = configuration;
            _customerService = customerService;
            _blobService = blobService;
        }

        // Action for Index page
        public IActionResult Index()
        {
            var model = new CustomerProfile();
            return View(model);
        }

        // Existing method to store customer info in Table storage and new SQL insertion
        // Modified HomeController.cs StoreTableInfo method
        [HttpPost]
        public async Task<IActionResult> StoreTableInfo(CustomerProfile profile)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for customer profile");
                return View("Index", profile);
            }

            try
            {
                // First try to insert into SQL database
                var sqlInsertSuccess = await _customerService.InsertCustomerAsync(profile);
                if (!sqlInsertSuccess)
                {
                    _logger.LogError("Failed to insert customer data into SQL database");
                    ModelState.AddModelError("", "Failed to save customer information");
                    return View("Index", profile);
                }

                // Then try to store in Azure Table
                using var httpClient = _httpClientFactory.CreateClient();
                var baseUrl = _configuration["AzureFunctions:StoreTableInfo"];

                if (string.IsNullOrEmpty(baseUrl))
                {
                    _logger.LogError("Azure Functions URL is not configured");
                    ModelState.AddModelError("", "Server configuration error");
                    return View("Index", profile);
                }

                var requestUri = $"{baseUrl}&tableName=CustomerProfiles&partitionKey={profile.PartitionKey}&rowKey={profile.RowKey}" +
                                $"&firstName={Uri.EscapeDataString(profile.FirstName)}" +
                                $"&SecondName={Uri.EscapeDataString(profile.SecondName)}" +
                                $"&phoneNumber={Uri.EscapeDataString(profile.PhoneNumber)}" +
                                $"&Email={Uri.EscapeDataString(profile.Email)}";

                var response = await httpClient.PostAsync(requestUri, null);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to store customer data in Azure Table: {StatusCode} - {Reason}",
                        response.StatusCode, response.ReasonPhrase);
                    ModelState.AddModelError("", "Failed to store customer information in cloud storage");
                    return View("Index", profile);
                }

                TempData["SuccessMessage"] = "Customer profile created successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing customer profile submission");
                ModelState.AddModelError("", "An error occurred while processing your request");
                return View("Index", profile);
            }
        }

        // Existing method to upload blob and new SQL insertion for blob data
        [HttpPost]
        public async Task<IActionResult> UploadBlob(IFormFile imageFile)
        {
            if (imageFile != null)
            {
                try
                {
                    // Call Azure function to upload the blob
                    using var httpClient = _httpClientFactory.CreateClient();
                    using var stream = imageFile.OpenReadStream();
                    var content = new StreamContent(stream);
                    content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(imageFile.ContentType);

                    var baseUrl = _configuration["AzureFunctions:UploadBlob"];
                    string url = $"{baseUrl}&blobName={imageFile.FileName}";
                    var response = await httpClient.PostAsync(url, content);

                    if (response.IsSuccessStatusCode)
                    {
                        // Convert image to byte array for SQL insertion
                        using (var memoryStream = new MemoryStream())
                        {
                            await imageFile.CopyToAsync(memoryStream);
                            var imageData = memoryStream.ToArray();

                            // Insert image data into SQL BlobTable
                            await _blobService.InsertBlobAsync(imageData);
                        }

                        return RedirectToAction("Index");
                    }
                    else
                    {
                        _logger.LogError($"Error submitting image: {response.ReasonPhrase}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Exception occurred while submitting image: {ex.Message}");
                }
            }
            else
            {
                _logger.LogError("No image file provided.");
            }

            return View("Index");
        }
    }
}
