using Google.Apis.Auth.OAuth2;
using Google.Cloud.AIPlatform.V1;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using CloudStoragePlatform.Core.ServiceContracts;
using Microsoft.Extensions.DependencyInjection;
using CloudStoragePlatform.Core.Domain.IdentityEntites;

namespace CloudStoragePlatform.Core.Services
{
    public class AiUpscaleProcessor : IAiUpscaleProcessor
    {
        private readonly IConfiguration _config;
        private readonly IServiceProvider _provider;
        private readonly UserIdentification userIdentification;

        public AiUpscaleProcessor(IConfiguration config, IServiceProvider serviceProvider, UserIdentification userIdentification)
        {
            _config = config;
            _provider = serviceProvider;
            this.userIdentification = userIdentification;
        }

        public async Task UpscaleDefault(Guid id)
        {
            string path = @"C:\Users\rusha\OneDrive\Pictures\who.png";

            // TODO move these details to config
            string projectID = "cloud-storage-platform-rushaan";
            string region = "us-central1";
            string publisher = "google";
            string model = "imagen-4.0-upscale-preview";

            Func<Task> upscaleJob = async () => 
            {
                await UpscaleImageAsync(Convert.ToBase64String(File.ReadAllBytes(path)), projectID, region, publisher, model);
            };

            ApplicationUser usr = userIdentification.User!;

            _= Task.Run(async () =>
            {
                using var scope = _provider.CreateScope();
                IFilesModificationService fms = scope.ServiceProvider.GetRequiredService<IFilesModificationService>();
                UserIdentification usrIdentification = scope.ServiceProvider.GetRequiredService<UserIdentification>();
                usrIdentification.User = usr;
                await MinRamThreshold.WaitForRamOrTimeoutAsync(upscaleJob);
            });
            

            // TODO most likely handle API ops in this fn
            // TODO since most AI features will need ram optimisation and background jobs, wise to have a singleton background orchestrator service for the same instead of repeating background & scope logic
        }


        public async Task UpscaleImageAsync(string b64, string projectId, string location, string publisher, string modelName)
        {
            string base64 = _config["GoogleServiceAccountJsonKey"];
            string saJson = Encoding.UTF8.GetString(Convert.FromBase64String(base64));

            using var doc = JsonDocument.Parse(saJson);
            var root = doc.RootElement;

            string clientEmail = root.GetProperty("client_email").GetString();
            string privateKeyPem = root.GetProperty("private_key").GetString();

            // 1. Creating RSA key from PEM
            RSA rsa = RSA.Create();
            rsa.ImportFromPem(privateKeyPem.ToCharArray()); // ImportFromPem accepts PEM including -----BEGIN PRIVATE KEY-----...


            // 2. Build ServiceAccountCredential initializer
            var initializer = new ServiceAccountCredential.Initializer(clientEmail)
            {
                Scopes = new[] { "https://www.googleapis.com/auth/cloud-platform" }
            };
            initializer.Key = rsa; // set RSA key to sign JWT


            // 3. Create credential and request access token
            var svcCred = new ServiceAccountCredential(initializer);
            bool success = await svcCred.RequestAccessTokenAsync(CancellationToken.None);
            if (!success) throw new Exception("Failed to get access token");

            string accessToken = svcCred.Token.AccessToken;
            Console.WriteLine("Access token length: " + accessToken.Length);



            // real upscale being done below:
            var endpoint = $"{location}-aiplatform.googleapis.com";

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            string url = $"https://{endpoint}/v1/projects/{projectId}/locations/{location}/publishers/{publisher}/models/{modelName}:predict";

            string request = BuildRequestJson(b64);
            var content = new StringContent(request, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(url, content);
            var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            string b64_upscaled = json.RootElement
                .GetProperty("predictions")[0]
                .GetProperty("bytesBase64Encoded")
                .GetString();
            File.WriteAllBytes("C:\\Users\\rusha\\OneDrive\\Pictures\\chheese", Convert.FromBase64String(b64_upscaled));
        }

        private string BuildRequestJson(string b64, int count = 1, string factor = "x2") 
        {
            return "{\r\n  \"instances\": [\r\n    {\r\n      \"prompt\": \"\",\r\n      \"image\": {\r\n        \"bytesBase64Encoded\": \"" + b64+"\"\r\n      }\r\n    }\r\n  ],\r\n  \"parameters\": {\r\n    \"sampleCount\": "+count+",\r\n    \"mode\": \"upscale\",\r\n    \"upscaleConfig\": {\r\n      \"upscaleFactor\": \""+factor+"\"\r\n    }\r\n  }\r\n}\r\n";
        }
    }
}
