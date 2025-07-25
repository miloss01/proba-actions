using DockerHubBackend.Dto.Request;
using DockerHubBackend.Dto.Response;
using DockerHubBackend.Models;

namespace DockerHubBackend.Services.Interface
{
    public interface IUserService
    {
        Task ChangePassword(ChangePasswordDto changePasswordDto);
        Task<BaseUserDTO> Register<TUser>(RegisterUserDto registerUserDto) where TUser : BaseUser;
        List<StandardUser> GetAllStandardUsers();
        void ChangeUserBadge(Badge badge, Guid userId);
    }
}
