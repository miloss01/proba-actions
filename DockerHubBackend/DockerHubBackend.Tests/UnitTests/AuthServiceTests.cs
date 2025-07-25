using DockerHubBackend.Dto.Request;
using DockerHubBackend.Exceptions;
using DockerHubBackend.Models;
using DockerHubBackend.Repository.Interface;
using DockerHubBackend.Security;
using DockerHubBackend.Services.Implementation;
using DockerHubBackend.Startup;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace DockerHubBackend.Tests.UnitTests
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IPasswordHasher<string>> _mockPasswordHasher;
        private readonly Mock<IJwtHelper> _mockJwtHelper;
        private readonly AuthService _service;
        private readonly Mock<IVerificationTokenRepository> _mockVerificationTokenRepository;
        private readonly Mock<IRandomTokenGenerator> _mockRandomTokenGenerator;
        private readonly Mock<ILogger<AuthService>> _mockLogger = new Mock<ILogger<AuthService>>();

        public AuthServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockPasswordHasher = new Mock<IPasswordHasher<string>>();
            _mockJwtHelper = new Mock<IJwtHelper>();
            _mockVerificationTokenRepository = new Mock<IVerificationTokenRepository>();
            _mockRandomTokenGenerator = new Mock<IRandomTokenGenerator>();
            _service = new AuthService(_mockUserRepository.Object, _mockJwtHelper.Object, _mockPasswordHasher.Object, _mockVerificationTokenRepository.Object, _mockRandomTokenGenerator.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsLoginResponseWithJwt()
        {

            var credentials = new LoginCredentialsDto { Email = "test@example.com", Password = "password123" };
            var user = new StandardUser {Username = "username", Id = Guid.NewGuid(), Email = credentials.Email, Password = "hashedPassword", LastPasswordChangeDate = DateTime.UtcNow };

            _mockUserRepository.Setup(repo => repo.GetUserWithTokenByEmail(credentials.Email)).ReturnsAsync(user);

            _mockPasswordHasher.Setup(hasher => hasher.VerifyHashedPassword(It.IsAny<string>(), user.Password, credentials.Password))
                      .Returns(PasswordVerificationResult.Success);


            _mockJwtHelper.Setup(jwt => jwt.GenerateToken(user.GetType().Name, user.Id.ToString(), user.Email, user.Username))
                         .Returns("dummyToken");


            var result = await _service.Login(credentials);

            Assert.NotNull(result);
            Assert.Equal("dummyToken", result.AccessToken);
        }

        [Fact]
        public async Task Login_UserNotFound_ThrowsBadRequestException()
        {
            var credentials = new LoginCredentialsDto { Email = "notfound@example.com", Password = "password123" };

            _mockUserRepository.Setup(repo => repo.GetUserWithTokenByEmail(credentials.Email)).ReturnsAsync((BaseUser?)null);

            var exception = await Assert.ThrowsAsync<UnauthorizedException>(() => _service.Login(credentials));
            Assert.Equal("Wrong email or password", exception.Message);
        }

        [Fact]
        public async Task Login_InvalidPassword_ThrowsBadRequestException()
        {
            var credentials = new LoginCredentialsDto { Email = "test@example.com", Password = "wrongPassword" };
            var user = new StandardUser { Username = "username", Id = Guid.NewGuid(), Email = credentials.Email, Password = "hashedPassword", LastPasswordChangeDate = DateTime.UtcNow};

            _mockUserRepository.Setup(repo => repo.GetUserWithTokenByEmail(credentials.Email)).ReturnsAsync(user);

            _mockPasswordHasher.Setup(hasher => hasher.VerifyHashedPassword(It.IsAny<string>(), user.Password, credentials.Password))
                      .Returns(PasswordVerificationResult.Failed);

            var exception = await Assert.ThrowsAsync<UnauthorizedException>(() => _service.Login(credentials));
            Assert.Equal("Wrong email or password", exception.Message);
        }

        [Fact]
        public async Task Login_UnverifiedSuperAdmin_ThrowsAccountVerificationRequiredException()
        {
            var credentials = new LoginCredentialsDto { Email = "test@example.com", Password = "password123" };
            var user = new SuperAdmin { Username = "username", Id = Guid.NewGuid(), Email = credentials.Email, Password = "hashedPassword", LastPasswordChangeDate = DateTime.UtcNow, IsVerified = false };
            string token = "randomToken";
            VerificationToken tokenObj = new VerificationToken { Token = token, User = user, UserId = user.Id, ValidUntil = DateTime.UtcNow };

            _mockUserRepository.Setup(repo => repo.GetUserWithTokenByEmail(credentials.Email)).ReturnsAsync(user);
            _mockPasswordHasher.Setup(hasher => hasher.VerifyHashedPassword(It.IsAny<string>(), user.Password, credentials.Password))
                        .Returns(PasswordVerificationResult.Success);
            _mockRandomTokenGenerator.Setup(generator => generator.GenerateVerificationToken(It.IsAny<int>()))
                .Returns(token);
            _mockVerificationTokenRepository.Setup(repo => repo.Update(It.IsAny<VerificationToken>()))
                .Returns(Task.FromResult<VerificationToken?>(tokenObj));
            _mockVerificationTokenRepository.Setup(repo => repo.Create(It.IsAny<VerificationToken>()))
                .Returns(Task.FromResult(tokenObj));

            var exception = await Assert.ThrowsAsync<AccountVerificationRequiredException>(() => _service.Login(credentials));
            Assert.Equal("Account verification required", exception.Message);
        }

        [Fact]
        public async Task Login_UnverifiedSuperAdmin_UpdatesVerificationToken()
        {
            var credentials = new LoginCredentialsDto { Email = "test@example.com", Password = "password123" };
            var user = new SuperAdmin { Username = "username", Id = Guid.NewGuid(), Email = credentials.Email, Password = "hashedPassword", LastPasswordChangeDate = DateTime.UtcNow, IsVerified = false };
            string newToken = "newToken";
            var oldCreatedAt = DateTime.UtcNow.AddHours(-2);
            var oldValidUntil = DateTime.UtcNow.AddHours(-1);
            VerificationToken tokenObj = new VerificationToken { Token = "oldToken", User = user, UserId = user.Id, ValidUntil = oldValidUntil, CreatedAt = oldCreatedAt };
            user.VerificationToken = tokenObj;

            _mockUserRepository.Setup(repo => repo.GetUserWithTokenByEmail(credentials.Email)).ReturnsAsync(user);
            _mockPasswordHasher.Setup(hasher => hasher.VerifyHashedPassword(It.IsAny<string>(), user.Password, credentials.Password))
                        .Returns(PasswordVerificationResult.Success);
            _mockRandomTokenGenerator.Setup(generator => generator.GenerateVerificationToken(It.IsAny<int>()))
                .Returns(newToken);
            _mockVerificationTokenRepository.Setup(repo => repo.Update(It.IsAny<VerificationToken>()))
                .Returns(Task.FromResult<VerificationToken?>(tokenObj));
            _mockVerificationTokenRepository.Setup(repo => repo.Create(It.IsAny<VerificationToken>()))
                .Returns(Task.FromResult(tokenObj));

            await Assert.ThrowsAsync<AccountVerificationRequiredException>(() => _service.Login(credentials));

            _mockVerificationTokenRepository.Verify(repo => repo.Update(It.IsAny<VerificationToken>()), Times.Once);
            _mockVerificationTokenRepository.Verify(repo => repo.Create(It.IsAny<VerificationToken>()), Times.Never);
            Assert.True(tokenObj.CreatedAt != oldCreatedAt);
            Assert.True(tokenObj.ValidUntil != oldValidUntil);
        }

        [Fact]
        public async Task Login_UnverifiedSuperAdmin_CreatesVerificationToken()
        {
            var credentials = new LoginCredentialsDto { Email = "test@example.com", Password = "password123" };
            var user = new SuperAdmin { Username = "username", Id = Guid.NewGuid(), Email = credentials.Email, Password = "hashedPassword", LastPasswordChangeDate = DateTime.UtcNow, IsVerified = false };
            string newToken = "newToken";
            VerificationToken? tokenObj = new VerificationToken { Token = newToken, User = user, UserId = user.Id, ValidUntil = DateTime.UtcNow.AddHours(-1), CreatedAt = DateTime.UtcNow.AddHours(-2) };

            _mockUserRepository.Setup(repo => repo.GetUserWithTokenByEmail(credentials.Email)).ReturnsAsync(user);
            _mockPasswordHasher.Setup(hasher => hasher.VerifyHashedPassword(It.IsAny<string>(), user.Password, credentials.Password))
                        .Returns(PasswordVerificationResult.Success);
            _mockRandomTokenGenerator.Setup(generator => generator.GenerateVerificationToken(It.IsAny<int>()))
                .Returns(newToken);
            _mockVerificationTokenRepository.Setup(repo => repo.Update(It.IsAny<VerificationToken>()))
                .Returns(Task.FromResult<VerificationToken?>(tokenObj));
            _mockVerificationTokenRepository.Setup(repo => repo.Create(It.IsAny<VerificationToken>()))
                .Returns(Task.FromResult(tokenObj));

            await Assert.ThrowsAsync<AccountVerificationRequiredException>(() => _service.Login(credentials));

            _mockVerificationTokenRepository.Verify(repo => repo.Create(It.Is<VerificationToken>(token =>
                token.Token == tokenObj.Token &&
                token.CreatedAt <= DateTime.UtcNow &&
                token.ValidUntil >= DateTime.UtcNow
            )), Times.Once);

            _mockVerificationTokenRepository.Verify(repo => repo.Update(It.IsAny<VerificationToken>()), Times.Never);
        }
    }
}
