using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Azure;
using MonedAppV3.Configuration;
using MonedAppV3.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAzureClients(factory =>
{
    factory.AddSecretClient(builder.Configuration.GetSection("KeyVault"));
});

SecretClient secretClient = builder.Services.BuildServiceProvider()
                                            .GetRequiredService<SecretClient>();

KeyVaultSecret secretStorageAccount = await secretClient.GetSecretAsync("storageaccount");

BlobServiceClient blobServiceClient = new BlobServiceClient(secretStorageAccount.Value);
builder.Services.AddSingleton(blobServiceClient);
builder.Services.AddTransient<ServiceBlobs>();

ServiceBlobs serviceBlobs = new ServiceBlobs(blobServiceClient);
AppImages appImages = await serviceBlobs.GetAppImagesAsync();
builder.Services.AddSingleton(appImages);

builder.Services.AddControllersWithViews(options => options.EnableEndpointRouting = false);
builder.Services.AddSession();
builder.Services.AddAuthentication(
        options =>
        {
            options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        }
    )
    .AddCookie();

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<ServiceMonedApp>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.UseStaticFiles();

app.UseMvc(
    routes =>
    {
        routes.MapRoute(
            name: "default",
            template: "{controller=Dashboard}/{action=Index}/{id?}"
        );
    });

app.Run();
