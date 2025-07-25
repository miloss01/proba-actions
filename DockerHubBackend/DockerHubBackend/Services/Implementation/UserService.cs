using DockerHubBackend.Dto.Request;
using DockerHubBackend.Dto.Response;
using DockerHubBackend.Exceptions;
using DockerHubBackend.Models;
using DockerHubBackend.Repository.Interface;
using DockerHubBackend.Services.Interface;
using Microsoft.AspNetCore.Identity;
using System.Net;

namespace DockerHubBackend.Services.Implementation
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IVerificationTokenRepository _verificationTokenRepository;
        private readonly IPasswordHasher<string> _passwordHasher;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, IVerificationTokenRepository verificationTokenRepository, IPasswordHasher<string> passwordHasher, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _verificationTokenRepository = verificationTokenRepository;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        public async Task ChangePassword(ChangePasswordDto changePasswordDto)
        {
            _logger.LogInformation("Attempting to change password using token: {Token}", changePasswordDto.Token);

            var token = await _verificationTokenRepository.GetTokenByValue(changePasswordDto.Token);
            if (token == null || token.ValidUntil < DateTime.UtcNow)
            {
                _logger.LogError("Invalid or expired token: {Token}", changePasswordDto.Token);
                throw new UnauthorizedException("Invalid token");
            }
            token.User.LastPasswordChangeDate = DateTime.UtcNow;
            token.User.Password = _passwordHasher.HashPassword(String.Empty, changePasswordDto.NewPassword);
            ((SuperAdmin)token.User).IsVerified = true;

            await _userRepository.Update(token.User);
            _logger.LogInformation("Password successfully changed for user: {UserEmail}", token.User.Email);
        }

        public List<StandardUser> GetAllStandardUsers()
        {
            _logger.LogInformation("Fetching all standard users.");
            var users = _userRepository.GetAllStandardUsers();
            _logger.LogInformation("Fetched {Count} standard users.", users.Count);
            return users;
        }

        public void ChangeUserBadge(Badge badge, Guid userId)
        {
            _logger.LogInformation("Changing badge for user with ID: {UserId} to {Badge}", userId, badge);
            _userRepository.ChangeUserBadge(badge, userId);
            _logger.LogInformation("Badge successfully updated for user with ID: {UserId}", userId);
        }

        public async Task<BaseUserDTO> Register<TUser>(RegisterUserDto registerUserDto) where TUser : BaseUser
        {
            _logger.LogInformation("Attempting to register a new user with email: {Email} and username: {Username}", registerUserDto.Email, registerUserDto.Username);
            if (await _userRepository.GetUserByEmail(registerUserDto.Email) != null)
            {
                _logger.LogError("Registration failed. Email {Email} is already in use.", registerUserDto.Email);
                throw new BadRequestException("An account with the given email already exists.");
            }

            if (await _userRepository.GetUserByUsername(registerUserDto.Username) != null)
            {
                _logger.LogError("Registration failed. Username {Username} is already in use.", registerUserDto.Username);
                throw new BadRequestException("The given username is already in use.");
            }

            var hashedPassword = _passwordHasher.HashPassword(string.Empty, registerUserDto.Password);

            var user = (TUser)Activator.CreateInstance(typeof(TUser),
                registerUserDto.Email,
                registerUserDto.Username,
                hashedPassword,
                registerUserDto.Location);

            await _userRepository.Create(user);
             _logger.LogInformation("User successfully registered with email: {Email}", user.Email);
            return new BaseUserDTO(user);
        }
    }
}
