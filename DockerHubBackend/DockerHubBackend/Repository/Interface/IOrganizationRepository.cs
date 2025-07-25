using DockerHubBackend.Dto.Request;
using DockerHubBackend.Dto.Response.Organization;
using DockerHubBackend.Models;
using DockerHubBackend.Repository.Utils;

namespace DockerHubBackend.Repository.Interface
{
    public interface IOrganizationRepository : ICrudRepository<Organization>
    {
        Task<Guid?> AddOrganization(AddOrganizationDto organization);
        Task<List<OrganizationOwnershipDto>?> GetUserOrganizations(string email);
        Task<Organization?> GetOrganizationById(Guid id);
        Task<OrganizationUsersDto> GetListUsersByOrganizationId(Guid organizationId);
        Task<string> AddMemberToOrganization(Guid organizationId, Guid userId);
        Task DeleteOrganization(Guid organizationId);
        Task UpdateOrganization(Guid organizationId, string imageLocation, string description);
        Task<Organization?> GetOrganizationByIdWithRepositories(Guid id);
    }
}
