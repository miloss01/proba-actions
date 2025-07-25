using DockerHubBackend.Services.Interface;
using System.Text;
using System.Text.Json;


namespace DockerHubBackend.Services.Implementation
{
    public class RegistryService : IRegistryService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public RegistryService(IConfiguration config)
        {
            _httpClient = new HttpClient();
            _config = config;
        }

        private async Task<string> GetAccessToken(string repository)
        {
            var username = _config["Registry:Username"];
            var password = _config["Registry:Password"];
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(username + ":" + password));

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(_config["Registry:AuthServerAddress"] + "/token?offline_token=true&service=uks-registry&scope=repository%3A" + repository + "%3Apush%2Cpull%2Cdelete"),
                Headers =
                {
                    { "Authorization", "Basic " + credentials },
                },
            };
            using (var response = await _httpClient.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();

                using var jsonDoc = JsonDocument.Parse(body);
                if (jsonDoc.RootElement.TryGetProperty("token", out var token))
                {
                    var tokenStr = token.GetString();
                    if(tokenStr == null)
                    {
                        throw new ArgumentNullException("Invalid token");
                    }
                    return tokenStr;
                }

                throw new Exception("Token not found in response.");
            }
        }

        public async Task DeleteDockerImage(string digest, string repository)
        {
            var token = GetAccessToken(repository);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(_config["Registry:RegistryAddress"] + "/v2/ubuntu/manifests/" + digest),
                Headers =
                {
                    { "Accept", "application/vnd.docker.distribution.manifest.v2+json" },
                    { "Authorization", "Bearer " + (await token) },
                }
            };

            using (var response = await _httpClient.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
            }
        }
    }
}
