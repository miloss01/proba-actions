using System.ComponentModel.DataAnnotations.Schema;
using DockerHubBackend.Dto.Response;

namespace DockerHubBackend.Models
{
    public class Team : BaseEntity
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public ICollection<StandardUser> Members { get; set; } = new HashSet<StandardUser>();
        public required Guid OrganizationId { get; set; }
        [ForeignKey(nameof(OrganizationId))]
        public required Organization Organization { get; set; }

    }
}
