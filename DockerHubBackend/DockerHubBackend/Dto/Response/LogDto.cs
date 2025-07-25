using Nest;
using System.Text.Json;

namespace DockerHubBackend.Dto.Response
{
    public class LogDto
    {
        public DateTime? Timestamp { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }

        public LogDto() { }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
