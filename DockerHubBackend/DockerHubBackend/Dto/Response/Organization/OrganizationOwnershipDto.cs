using DockerHubBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DockerHubBackend.Dto.Response.Organization
{
    public class OrganizationOwnershipDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string ImageLocation { get; set; }
        public DateTime CreatedAt { get; set; }
        public string OwnerEmail { get; set; }
        public bool IsOwner { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
