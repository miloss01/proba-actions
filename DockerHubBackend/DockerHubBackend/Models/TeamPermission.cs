using System.ComponentModel.DataAnnotations.Schema;

namespace DockerHubBackend.Models
{
    public class TeamPermission : BaseEntity
    {
        public PermissionType Permission {  get; set; }
        public required Guid TeamId { get; set; }
        [ForeignKey(nameof(TeamId))]
        public required Team Team { get; set; }

        public required Guid RepositoryId { get; set; }
        [ForeignKey(nameof(RepositoryId))]
        public required DockerRepository Repository { get; set; }
    }
}
