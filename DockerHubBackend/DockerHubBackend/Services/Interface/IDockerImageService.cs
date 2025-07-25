using DockerHubBackend.Dto.Request;
using DockerHubBackend.Dto.Response;
using DockerHubBackend.Models;

namespace DockerHubBackend.Services.Interface
{
    public interface IDockerImageService
    {
        public PageDTO<DockerImage> GetDockerImages(int page, int pageSize, string? searchTerm, string? badges);
		public Task DeleteDockerImage(Guid id);

	}
}
