using System.Text.Json;

namespace DockerHubBackend.Dto.Request
{
    public class AddMemberDto
    {
        public Guid OrganizationId { get; set; }
        public Guid UserId { get; set; }

        public AddMemberDto() { }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
