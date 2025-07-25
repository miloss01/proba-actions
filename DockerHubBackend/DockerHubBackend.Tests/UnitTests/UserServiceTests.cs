using DockerHubBackend.Dto.Request;
using DockerHubBackend.Dto.Response;
using DockerHubBackend.Exceptions;
using DockerHubBackend.Models;
using DockerHubBackend.Repository.Interface;
using DockerHubBackend.Services.Implementation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Npgsql.TypeMapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DockerHubBackend.Tests.UnitTests
{

    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IVerificationTokenRepository> _mockVerificationTokenRepository;
        private readonly Mock<IPasswordHasher<string>> _mockPasswordHasher;
        private readonly UserService _service;
        private readonly Mock<ILogger<UserService>> _mockLogger = new Mock<ILogger<UserService>>();

        public UserServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockVerificationTokenRepository = new Mock<IVerificationTokenRepository>();
            _mockPasswordHasher = new Mock<IPasswordHasher<string>>();
            _service = new UserService(_mockUserRepository.Object, _mockVerificationTokenRepository.Object, _mockPasswordHasher.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task ChangePassword_NonExistingTokenThrowsUnauthorizedException()
        {
            var changePasswordDto = new ChangePasswordDto { NewPassword = "password", Token = "token" };

            _mockVerificationTokenRepository.Setup(repo => repo.GetTokenByValue(It.IsAny<string>())).ReturnsAsync((VerificationToken?)null);

            var exception = await Assert.ThrowsAsync<UnauthorizedException>(() => _service.ChangePassword(changePasswordDto));
            Assert.Equal("Invalid token", exception.Message);
        }

        [Fact]
        public async Task ChangePassword_ExpiredTokenThrowsUnauthorizedException()
        {
            var changePasswordDto = new ChangePasswordDto { NewPassword = "password", Token = "token" };
            var user = new SuperAdmin { Username = "SuperAdmin", Id = Guid.NewGuid(), Email = "email@email.com", Password = "hashedPassword", LastPasswordChangeDate = DateTime.UtcNow, IsVerified = false };
            var verificationToken = new VerificationToken { Token = "token", ValidUntil = DateTime.UtcNow.AddHours(-1), User = user, UserId = user.Id};

            _mockVerificationTokenRepository.Setup(repo => repo.GetTokenByValue(It.IsAny<string>())).ReturnsAsync(verificationToken);

            var exception = await Assert.ThrowsAsync<UnauthorizedException>(() => _service.ChangePassword(changePasswordDto));
            Assert.Equal("Invalid token", exception.Message);
        }

        [Fact]
        public async Task ChangePassword_ValidTokenUpdatesUser()
        {
            var changePasswordDto = new ChangePasswordDto { NewPassword = "newPassword", Token = "token" };
            var user = new SuperAdmin { Username = "SuperAdmin", Id = Guid.NewGuid(), Email = "email@email.com", Password = "hashedPassword", LastPasswordChangeDate = null, IsVerified = false };
            var verificationToken = new VerificationToken { Token = "token", ValidUntil = DateTime.UtcNow.AddHours(1), User = user, UserId = user.Id };

            _mockVerificationTokenRepository.Setup(repo => repo.GetTokenByValue("token")).ReturnsAsync(verificationToken);
            _mockPasswordHasher.Setup(hasher => hasher.HashPassword(It.IsAny<string>(), changePasswordDto.NewPassword))
                        .Returns("newHashedPassword");
            
            await _service.ChangePassword(changePasswordDto);

            Assert.Equal("newHashedPassword", user.Password);
            Assert.True(user.IsVerified);
            Assert.True(user.LastPasswordChangeDate.HasValue);
            _mockUserRepository.Verify(repo => repo.Update(It.IsAny<BaseUser>()), Times.Once);
        }

        [Theory]
        [InlineData(typeof(StandardUser))]
        [InlineData(typeof(Admin))]
        public async Task RegisterUser_ExistingEmailThrowsBadRequestException(Type userType)
        {
            RegisterUserDto registerUserDto = new RegisterUserDto
            {
                Email = "user@email.com",
                Password = "password",
                Location = "location",
                Username = "username",
            };

            _mockUserRepository.Setup(repo => repo.GetUserByEmail(registerUserDto.Email))
                .ReturnsAsync(new StandardUser { Email = registerUserDto.Email, Username="user123", Password="password123"});

            var method = typeof(UserService).GetMethod("Register").MakeGenericMethod(userType);
            var exception = await Assert.ThrowsAsync<BadRequestException>(() => (Task<BaseUserDTO>)method.Invoke(_service, new object[] { registerUserDto }));
            Assert.Equal("An account with the given email already exists.", exception.Message);
        }

        [Theory]
        [InlineData(typeof(StandardUser))]
        [InlineData(typeof(Admin))]
        public async Task RegisterUser_ExistingUsernameThrowsBadRequestException(Type userType)
        {
            RegisterUserDto registerUserDto = new RegisterUserDto
            {
                Email = "user@email.com",
                Password = "password",
                Location = "location",
                Username = "username",
            };

            _mockUserRepository.Setup(repo => repo.GetUserByEmail(registerUserDto.Email))
                .ReturnsAsync((BaseUser?)null);

            _mockUserRepository.Setup(repo => repo.GetUserByUsername(registerUserDto.Username))
                .ReturnsAsync(new StandardUser { Email = "user123@email.com", Username = registerUserDto.Username, Password = "password123" });

            var method = typeof(UserService).GetMethod("Register").MakeGenericMethod(userType);
            var exception = await Assert.ThrowsAsync<BadRequestException>(() => (Task<BaseUserDTO>)method.Invoke(_service, new object[] { registerUserDto }));
            Assert.Equal("The given username is already in use.", exception.Message);
        }

        [Theory]
        [InlineData(typeof(StandardUser))]
        [InlineData(typeof(Admin))]
        public async Task RegisterUser_UniqueEmailAndUsernameCreateUser(Type userType)
        {
            RegisterUserDto registerUserDto = new RegisterUserDto
            {
                Email = "user@email.com",
                Password = "password",
                Location = "location",
                Username = "username",
            };

            _mockUserRepository.Setup(repo => repo.GetUserByEmail(registerUserDto.Email))
                .ReturnsAsync((BaseUser?)null);

            _mockUserRepository.Setup(repo => repo.GetUserByUsername(registerUserDto.Username))
                .ReturnsAsync((BaseUser?)null);

            _mockPasswordHasher.Setup(hasher => hasher.HashPassword(It.IsAny<string>(), registerUserDto.Password))
                .Returns("hashedPassword");

            var user = Activator.CreateInstance(userType) as BaseUser;
            if (user != null)
            {
                user.Email = registerUserDto.Email;
                user.Username = registerUserDto.Username;
                user.Password = "hashedPassword";
                user.Location = registerUserDto.Location;
            }

            var savedUser = Activator.CreateInstance(userType) as BaseUser;
            if (savedUser != null)
            {
                savedUser.Email = registerUserDto.Email;
                savedUser.Username = registerUserDto.Username;
                savedUser.Password = "hashedPassword";
                savedUser.Location = registerUserDto.Location;
            }

            _mockUserRepository.Setup(repo => repo.Create(It.IsAny<BaseUser>()))
                .ReturnsAsync(savedUser);

            var method = typeof(UserService).GetMethod("Register").MakeGenericMethod(userType);
            var result = await (Task<BaseUserDTO>)method.Invoke(_service, new object[] { registerUserDto });
            var expectedResult = new BaseUserDTO(savedUser);

            Assert.Equal(expectedResult.Email, result.Email);
            Assert.Equal(expectedResult.Username, result.Username);
            Assert.Equal(expectedResult.Location, result.Location);
            _mockUserRepository.Verify(repo => repo.Create(It.IsAny<BaseUser>()), Times.Once);
        }

        [Fact]
        public void GetAllStandardUsers_GetOnlyStandardUsers_ReturnsAllStandardUsers()
        {
            var standardUser1 = new StandardUser { Id = Guid.NewGuid(), Username = "User1", Password = "pass1", Email = "user1@email.com" };
            var standardUser2 = new StandardUser { Id = Guid.NewGuid(), Username = "User2", Password = "pass2", Email = "user2@email.com" };

            var standardUsers = new List<StandardUser>();
            standardUsers.Add(standardUser1);
            standardUsers.Add(standardUser2);

            _mockUserRepository.Setup(userRepository => userRepository.GetAllStandardUsers()).Returns(standardUsers);

            var result = _service.GetAllStandardUsers();

            Assert.Equal(standardUsers, result);
        }

        [Fact]
        public void ChangeUserBadge_ChangeSpecificUserBadgeToProvidedBadge_ReturnsNothing()
        {
            var standardUser = new StandardUser { Id = Guid.NewGuid(), Username = "User1", Password = "pass1", Email = "user1@email.com", Badge = Badge.NoBadge };
            var newBadge = Badge.VerifiedPublisher;

            _mockUserRepository.Setup(userRepository => userRepository.ChangeUserBadge(newBadge, standardUser.Id)).Verifiable();

            _service.ChangeUserBadge(newBadge, standardUser.Id);

            _mockUserRepository.Verify(
                userRepository => userRepository.ChangeUserBadge(newBadge, standardUser.Id),
                Times.Once
            );
        }
    }
}
