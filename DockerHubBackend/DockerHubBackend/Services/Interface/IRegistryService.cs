
namespace DockerHubBackend.Services.Interface
{
    public interface IRegistryService
    {
        Task DeleteDockerImage(string digest, string repository);
    }
}
