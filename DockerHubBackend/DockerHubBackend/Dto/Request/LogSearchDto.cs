using System.Text.Json;

namespace DockerHubBackend.Dto.Request
{
    public class LogSearchDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Level { get; set; }
        public string? Query { get; set; }

        public LogSearchDto() { }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
