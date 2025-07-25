using Microsoft.Extensions.Primitives;

namespace DockerHubBackend.Dto.Request
{
    public class TeamPermissionRequestDto
    {
        public required Guid TeamId { get; set; }
        public required Guid RepositoryId { get; set; }
        public required string Permission {  get; set; }
    }
}
