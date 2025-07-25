using System.Text.Json;

namespace DockerHubBackend.Dto.Response
{
    public class LoginResponse
    {
        public string AccessToken { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
