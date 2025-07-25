using DockerHubBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DockerHubBackend.Dto.Response.Organization
{
    public class OrganizationUsersDto
    {
        public List<MemberDto> Members { get; set; }

        public List<MemberDto> OtherUsers { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
