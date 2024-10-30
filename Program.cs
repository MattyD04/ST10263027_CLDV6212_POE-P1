using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using ST10263027_CLDV6212_POE_2_.Services;

var builder = WebApplication.CreateBuilder(args);

// Register services for dependency injection
builder.Services.AddHttpClient();
builder.Services.AddControllersWithViews();

// Add configuration
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

// Register services
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<BlobService>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("DefaultConnection"); // Make sure this matches your appsettings.json
    return new BlobService(connectionString);
});
builder.Services.AddLogging();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
