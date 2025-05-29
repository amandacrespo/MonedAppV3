using Amazon.S3;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Azure;
using MonedAppV3.Services;
using Newtonsoft.Json;
using NugetMonedAppAws.Models;

var builder = WebApplication.CreateBuilder(args);

var jsonSecrets = HelperSecretManager.GetSecretsAsync().GetAwaiter().GetResult();
KeysModel keysModel = JsonConvert.DeserializeObject<KeysModel>(jsonSecrets);
services.AddSingleton<KeysModel>(keysModel);

builder.Services.AddAWSService<IAmazonS3>();
builder.Services.AddTransient<ServiceStorageS3>();

using (var tempProvider = builder.Services.BuildServiceProvider()) {
    var serviceStorage = tempProvider.GetRequiredService<ServiceStorageS3>();
    var appImages = await serviceStorage.GetAppImagesAsync();
    builder.Services.AddSingleton(appImages);
}

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
