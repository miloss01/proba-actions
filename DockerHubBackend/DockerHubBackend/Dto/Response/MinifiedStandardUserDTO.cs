using DockerHubBackend.Models;
using System.Text.Json;

namespace DockerHubBackend.Dto.Response
{
    public class MinifiedStandardUserDTO
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Badge { get; set; }


        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
