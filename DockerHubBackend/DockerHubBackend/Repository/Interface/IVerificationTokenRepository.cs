using DockerHubBackend.Models;
using DockerHubBackend.Repository.Utils;

namespace DockerHubBackend.Repository.Interface
{
    public interface IVerificationTokenRepository : ICrudRepository<VerificationToken>
    {
        Task<VerificationToken?> GetTokenByUserId(Guid userId);
        Task<VerificationToken?> GetTokenByValue(string token);
    }
}
