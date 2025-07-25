using System.Text.Json;

namespace DockerHubBackend.Dto.Response
{
    public class DockerImageDTO
    {
        public string ImageId { get; set; }
        public string RepositoryName { get; set; }
        public string RepositoryId { get; set; }
        public string Badge { get; set; }
        public int StarCount { get; set; }
        public string Description { get; set; }
        public List<string> Tags { get; set; }
        public string LastPush { get; set; }
        public string Owner { get; set; }
        public string CreatedAt { get; set; }
        public string Digest { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}