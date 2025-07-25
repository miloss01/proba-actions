using Amazon.S3.Model;
using DockerHubBackend.Dto.Response;
using DockerHubBackend.Models;
using DockerHubBackend.Repository.Utils;

namespace DockerHubBackend.Repository.Interface
{
    public interface IDockerRepositoryRepository : ICrudRepository<DockerRepository>
    {
        Task<DockerRepository?> GetDockerRepositoryById(Guid id);
        Task<DockerRepository?> GetDockerRepositoryByIdWithImages(Guid id);

        Task<List<DockerRepository>?> GetRepositoriesByUserOwnerId(Guid id);

        Task<List<DockerRepository>?> GetRepositoriesByOrganizationOwnerId(Guid id);

        public DockerRepository GetFullDockerRepositoryById(Guid id);
        public List<DockerRepository> GetStarRepositoriesForUser(Guid userId);
        public List<DockerRepository> GetPrivateRepositoriesForUser(Guid userId);
        public List<DockerRepository> GetOrganizationRepositoriesForUser(Guid userId);
        public List<DockerRepository> GetAllRepositoriesForUser(Guid userId);
        public void AddStarRepository(Guid userId, Guid repositoryId);
        public void RemoveStarRepository(Guid userId, Guid repositoryId);
        public PageDTO<DockerRepository> GetDockerRepositories(int page, int pageSize, string? searchTerm, string? badges);
    }
}
