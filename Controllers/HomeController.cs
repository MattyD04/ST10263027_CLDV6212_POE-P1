using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ST10263027_CLDV6212_POE_2_.Models;
using ST10263027_CLDV6212_POE_2_.Services;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ST10263027_CLDV6212_POE_2_.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        private readonly CustomerService _customerService;
        private readonly OrderService _orderService; // New OrderService instance
        private readonly BlobService _blobService;

        public HomeController(IHttpClientFactory httpClientFactory, ILogger<HomeController> logger,
            IConfiguration configuration, CustomerService customerService, OrderService orderService) // Injecting OrderService
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _configuration = configuration;
            _customerService = customerService;
            _orderService = orderService; // Initializing OrderService

            // Initialize BlobService with the connection string
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            _blobService = new BlobService(connectionString);
        }

        public IActionResult Index()
        {
            var model = new CustomerProfile();
            return View(model);
        }

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

        [HttpPost]
        public async Task<IActionResult> InsertOrder(OrderProfile order)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for order");
                return View("Index", order);
            }

            try
            {
                // Insert order into SQL database
                var sqlInsertSuccess = await _orderService.InsertOrderAsync(order);
                if (!sqlInsertSuccess)
                {
                    _logger.LogError("Failed to insert order data into SQL database");
                    ModelState.AddModelError("", "Failed to save order information");
                    return View("Index", order);
                }

                TempData["SuccessMessage"] = "Order created successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing order submission");
                ModelState.AddModelError("", "An error occurred while processing your order");
                return View("Index", order);
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadBlob(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                _logger.LogError("No image file provided or file is empty");
                ModelState.AddModelError("", "Please select a valid image file");
                return View("Index");
            }

            try
            {
                // Convert image to byte array for SQL insertion
                using (var memoryStream = new MemoryStream())
                {
                    await imageFile.CopyToAsync(memoryStream);
                    var imageData = memoryStream.ToArray();

                    // Insert image data into SQL BlobTable
                    var sqlInsertSuccess = await _blobService.InsertBlobAsync(imageData);

                    if (!sqlInsertSuccess)
                    {
                        _logger.LogError("Failed to insert image data into SQL database");
                        ModelState.AddModelError("", "Failed to save image to database");
                        return View("Index");
                    }

                    // Call Azure function to upload the blob if necessary
                    using var httpClient = _httpClientFactory.CreateClient();
                    var baseUrl = _configuration["AzureFunctions:UploadBlob"];
                    if (string.IsNullOrEmpty(baseUrl))
                    {
                        _logger.LogError("Azure Functions URL is not configured");
                        ModelState.AddModelError("", "Server configuration error");
                        return View("Index");
                    }

                    string url = $"{baseUrl}&blobName={Uri.EscapeDataString(imageFile.FileName)}";
                    var content = new StreamContent(memoryStream);
                    content.Headers.ContentType = new MediaTypeHeaderValue(imageFile.ContentType);

                    var response = await httpClient.PostAsync(url, content);
                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogError("Failed to upload image to Azure Blob Storage: {StatusCode} - {Reason}",
                            response.StatusCode, response.ReasonPhrase);
                        ModelState.AddModelError("", "Failed to upload image to cloud storage");
                    }
                    else
                    {
                        TempData["SuccessMessage"] = "Image uploaded successfully!";
                    }

                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing image upload: {Message}", ex.Message);
                ModelState.AddModelError("", "An error occurred while processing your image");
            }

            return View("Index");
        }
    }
}
