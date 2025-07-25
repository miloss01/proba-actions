using DockerHubBackend.Dto.Request;
using DockerHubBackend.Dto.Response;
using DockerHubBackend.Dto.Response.Organization;
using DockerHubBackend.Models;

namespace DockerHubBackend.Services.Interface
{
    public interface ITeamService
    {
        public Task<ICollection<TeamDto>?> GetTeams(Guid organizationId);

        public Task<TeamDto> Create(TeamDto teamDto);

        public Task<TeamDto?> Get(Guid id);

        public Task<TeamDto> AddMembers(Guid teamId, ICollection<MemberDto> memberDtos);

        public Task<TeamPermissionResponseDto> AddPermissions(TeamPermissionRequestDto teamPermissionDto);

        public Task<TeamDto> Update(TeamDto teamDto, Guid id);

        public Task<TeamDto> Delete(Guid id);

        public Task<ICollection<TeamPermission>> GetTeamPermissions(Guid id);
        
    }
}
