using System.Text.Json;

namespace DockerHubBackend.Dto.Request
{
    public class UpdateImageDto
    {
        public string OldFileName { get; set; }
        public string NewFileName { get; set; }

        public UpdateImageDto() { }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
