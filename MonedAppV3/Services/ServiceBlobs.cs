using Azure.Storage.Blobs;
using MonedAppV3.Configuration;

namespace MonedAppV3.Services
{
    public class ServiceBlobs
    {
        private BlobServiceClient client;

        public ServiceBlobs(BlobServiceClient client) {
            this.client = client;
        }

        public string GetBlobUrl(string containerName, string blobName) {
            BlobContainerClient container = this.client.GetBlobContainerClient(containerName);
            BlobClient blob = container.GetBlobClient(blobName);
            return blob.Uri.AbsoluteUri;
        }

        public async Task<AppImages> GetAppImagesAsync() {
            var container = "imagenesestaticas";

            var images = new AppImages();
            images.Logo = GetBlobUrl(container, "MonedApp6-dark.png");
            images.Perfil = GetBlobUrl(container, "foto-perfil.png");
            images.ConCuenta = GetBlobUrl(container, "img-cuentas.jpeg");
            images.SinCuenta = GetBlobUrl(container, "sincuenta.png");
            images.AccionesRapidas = GetBlobUrl(container, "accionesrapidas.png");
            images.Denegado = GetBlobUrl(container, "denegado.png");

            return images;
        }
    }
}
