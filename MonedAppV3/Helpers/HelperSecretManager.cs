using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Amazon;
using System.Text.Json;

namespace MonedAppV3.Helpers
{
    public class HelperSecretManager
    {
        public static async Task<string> GetSecretsAsync() {
            string secretName = "secrets-moned-app";
            string region = "us-east-1";

            var client = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(region));

            var request = new GetSecretValueRequest
            {
                SecretId = secretName,
                VersionStage = "AWSCURRENT"
            };

            var response = await client.GetSecretValueAsync(request);
            var secretString = response.SecretString;

            return secretString;
        }
    }
}
