using Amazon.S3.Model;
using Amazon.S3;
using Azure.Storage.Blobs;
using MonedAppV3.Configuration;
using System.Net;

namespace MonedAppV3.Services
{
    public class ServiceStorageS3
    {
        private readonly string BucketUrl;

        public ServiceStorageS3(IConfiguration configuration, IAmazonS3 s3Client) {
            this.BucketUrl = configuration.GetValue<string>("AWS:BucketUrl");
        }

        public async Task<AppImages> GetAppImagesAsync() {
            var images = new AppImages
            {
                Logo = GenerateS3Url("MonedApp6-dark.png"),
                Perfil = GenerateS3Url("foto-perfil.png"),
                ConCuenta = GenerateS3Url("img-cuentas.jpeg"),
                SinCuenta = GenerateS3Url("sincuenta.png"),
                AccionesRapidas = GenerateS3Url("accionesrapidas.png"),
                Denegado = GenerateS3Url("denegado.png")
            };
            return images;
        }

        private string GenerateS3Url(string key) {
            return $"{this.BucketUrl}{key}";
        }
    }
}
