using System.Text.Json;

namespace DockerHubBackend.Dto.Request
{
    public class ImageDto
    {
        public string FileName { get; set; }

        public ImageDto() { }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
