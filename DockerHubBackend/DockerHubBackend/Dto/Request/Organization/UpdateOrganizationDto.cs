using System.Text.Json;

namespace DockerHubBackend.Dto.Request
{
    public class UpdateOrganizationDto
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public string ImageLocation { get; set; }

        public UpdateOrganizationDto() { }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
