using DockerHubBackend.Models;
using DockerHubBackend.Repository.Utils;

namespace DockerHubBackend.Repository.Interface
{
    public interface IUserRepository : ICrudRepository<BaseUser>
    {
        Task<BaseUser?> GetUserByEmail(string email);
        Task<BaseUser?> GetUserByUsername(string username);
        Task<BaseUser?> GetUserWithTokenByEmail(string email);
        Task<BaseUser?> GetUserById(Guid id);
        List<StandardUser> GetAllStandardUsers();
        void ChangeUserBadge(Badge badge, Guid userId);
    }
}
