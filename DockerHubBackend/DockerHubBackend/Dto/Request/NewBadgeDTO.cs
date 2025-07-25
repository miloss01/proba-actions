using System.Text.Json;

namespace DockerHubBackend.Dto.Request
{
    public class NewBadgeDTO
    {
        public string Badge { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
