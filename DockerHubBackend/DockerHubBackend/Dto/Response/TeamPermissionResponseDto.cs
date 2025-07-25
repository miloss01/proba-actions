using System.Diagnostics.CodeAnalysis;
using DockerHubBackend.Models;

namespace DockerHubBackend.Dto.Response
{
    public class TeamPermissionResponseDto
    {
        public required string TeamName {  get; set; }
        public required string RepositoryName { get; set; }
        public required string Permission {  get; set; }

        public TeamPermissionResponseDto() { }

        [SetsRequiredMembers]
        public TeamPermissionResponseDto(TeamPermission tp)
        {
            TeamName = tp.Team.Name;
            RepositoryName = tp.Repository.Name;
            Permission = tp.Permission.ToString();
        }
    }
}
