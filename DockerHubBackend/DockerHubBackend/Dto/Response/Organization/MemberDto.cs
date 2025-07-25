using DockerHubBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DockerHubBackend.Dto.Response.Organization
{
    public class MemberDto
    {
        public Guid Id { get; set; }

        //public string Name { get; set; }

        public string Email { get; set; }
        public bool IsOwner { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
