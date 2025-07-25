using System.Collections.ObjectModel;
using DockerHubBackend.Models;
using DockerHubBackend.Repository.Utils;
using Microsoft.VisualBasic;
using DockerHubBackend.Dto.Response;
using DockerHubBackend.Dto.Request;
using System.Runtime.CompilerServices;

namespace DockerHubBackend.Repository.Interface
{
    public interface ITeamRepository : ICrudRepository<Team>
    {

        Task<ICollection<TeamDto>> GetByOrganizationId(Guid organizationId);

        Task<Team?> Get(Guid id);

        Task<Team> GetByName(string name);

        Task<ICollection<StandardUser>> GetMembers(Guid id);

        void AddPermissions(TeamPermission teamPermission);

        TeamPermission? GetTeamPermission(Guid repositoryId, Guid id);

        Task<ICollection<TeamPermission>> GetTeamPermissions(Guid id);

        Task<Team?> GetByOrgIdAndTeamName(Guid orgId, string teamName);
    }
}
