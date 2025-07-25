using Amazon.S3.Model;
using DockerHubBackend.Dto.Request;
using DockerHubBackend.Dto.Response.Organization;
using DockerHubBackend.Models;
using DockerHubBackend.Repository.Interface;
using DockerHubBackend.Repository.Utils;
using DockerHubBackend.Services.Implementation;
using DockerHubBackend.Services.Interface;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DockerHubBackend.Tests.UnitTests
{
    public class OrganizationServiceTests
    {
        private readonly Mock<IOrganizationRepository> _orgRepoMock;
        private readonly Mock<ILogger<OrganizationService>> _loggerMock;
        private readonly Mock<IDockerRepositoryService> _dockerRepoServiceMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly OrganizationService _orgService;

        public OrganizationServiceTests()
        {
            _orgRepoMock = new Mock<IOrganizationRepository>();
            _loggerMock = new Mock<ILogger<OrganizationService>>();
            _dockerRepoServiceMock = new Mock<IDockerRepositoryService>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(Mock.Of<IDbContextTransaction>());
            _orgService = new OrganizationService(_orgRepoMock.Object, _loggerMock.Object, _dockerRepoServiceMock.Object, _unitOfWorkMock.Object);
        }

        [Fact]
        public async Task AddOrganization_ValidInput_ShouldReturnId()
        {
            var dto = new AddOrganizationDto { Name = "Test Org" };
            var expectedId = Guid.NewGuid();
            _orgRepoMock.Setup(r => r.AddOrganization(dto)).ReturnsAsync(expectedId);

            var result = await _orgService.AddOrganization(dto);

            Assert.Equal(expectedId, result);
        }

        [Fact]
        public async Task AddOrganization_NullResult_ShouldReturnNull()
        {
            var dto = new AddOrganizationDto { Name = "Invalid Org" };
            _orgRepoMock.Setup(r => r.AddOrganization(dto)).ReturnsAsync((Guid?)null);

            var result = await _orgService.AddOrganization(dto);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetOrganizations_ValidEmail_ShouldReturnList()
        {
            var email = "user@example.com";
            var expected = new List<OrganizationOwnershipDto> { new() { Name = "Org1" } };
            _orgRepoMock.Setup(r => r.GetUserOrganizations(email)).ReturnsAsync(expected);

            var result = await _orgService.GetOrganizations(email);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Org1", result[0].Name);
        }

        [Fact]
        public async Task GetOrganizations_InvalidEmail_ShouldReturnNull()
        {
            _orgRepoMock.Setup(r => r.GetUserOrganizations("none")).ReturnsAsync((List<OrganizationOwnershipDto>?)null);

            var result = await _orgService.GetOrganizations("none");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetOrganizationById_ValidId_ShouldReturnOrganization()
        {
            var id = Guid.NewGuid();
            var ownerId = Guid.NewGuid();

            var expected = new Organization
            {
                Id = id,
                Name = "Org",
                Description = "Opis organizacije",
                ImageLocation = "/images/org.png",
                OwnerId = ownerId,
                Owner = new StandardUser
                {
                    Id = ownerId,
                    Username = "owneruser",
                    Email = "owner@example.com",
                    Password = "securepass"
                },
                Members = new List<StandardUser>(),
                Repositories = new List<DockerRepository>(),
                Teams = new List<Team>()
            };
            _orgRepoMock.Setup(r => r.GetOrganizationById(id)).ReturnsAsync(expected);

            var result = await _orgService.GetOrganizationById(id);

            Assert.NotNull(result);
            Assert.Equal("Org", result.Name);
        }

        [Fact]
        public async Task GetOrganizationById_InvalidId_ShouldReturnNull()
        {
            _orgRepoMock.Setup(r => r.GetOrganizationById(It.IsAny<Guid>())).ReturnsAsync((Organization?)null);

            var result = await _orgService.GetOrganizationById(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task AddMemberToOrganization_ValidIds_ShouldReturnSuccessMessage()
        {
            var orgId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            _orgRepoMock.Setup(r => r.AddMemberToOrganization(orgId, userId))
            .ReturnsAsync("User added to organization successfully.");

            var result = await _orgService.AddMemberToOrganization(orgId, userId);

            Assert.Equal("User added to organization successfully.", result);
        }

        [Fact]
        public async Task AddMemberToOrganization_UserNotFound_ShouldReturnError()
        {
            var orgId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            _orgRepoMock.Setup(r => r.AddMemberToOrganization(orgId, userId))
            .ReturnsAsync("User not found.");

            var result = await _orgService.AddMemberToOrganization(orgId, userId);

            Assert.Equal("User not found.", result);
        }

        [Fact]
        public async Task DeleteOrganization_WhenOrganizationExists_ShouldDeleteRepositoriesAndOrganization()
        {
            var orgId = Guid.NewGuid();
            var repo1 = new DockerRepository { Id = Guid.NewGuid(), Name = "repo 1" };
            var repo2 = new DockerRepository { Id = Guid.NewGuid(), Name = "repo 2" };
            var org = new Organization
            {
                Id = orgId,
                Name = "TestOrg",
                ImageLocation = "img",
                OwnerId = Guid.NewGuid(),
                Repositories = new List<DockerRepository> { repo1, repo2 }
            };

            _orgRepoMock.Setup(r => r.GetOrganizationByIdWithRepositories(orgId)).ReturnsAsync(org);
            _orgRepoMock.Setup(r => r.DeleteOrganization(orgId)).Returns(Task.CompletedTask);
            _dockerRepoServiceMock.Setup(r => r.DeleteDockerRepository(It.IsAny<Guid>())).Returns(Task.CompletedTask);

            await _orgService.DeleteOrganization(orgId);

            _dockerRepoServiceMock.Verify(r => r.DeleteDockerRepository(repo1.Id), Times.Once);
            _dockerRepoServiceMock.Verify(r => r.DeleteDockerRepository(repo2.Id), Times.Once);
            _orgRepoMock.Verify(r => r.DeleteOrganization(orgId), Times.Once);
            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateOrganization_ShouldCallRepository()
        {
            var id = Guid.NewGuid();
            var image = "img.png";
            var desc = "desc";

            await _orgService.UpdateOrganization(id, image, desc);

            _orgRepoMock.Verify(r => r.UpdateOrganization(id, image, desc), Times.Once);
        }

        [Fact]
        public async Task GetListUsersByOrganizationId_ShouldReturnUsersDto()
        {
            var orgId = Guid.NewGuid();
            var expected = new OrganizationUsersDto
            {
                Members = new List<MemberDto>
                {
                    new MemberDto { Id = Guid.NewGuid(), Email = "member@example.com", IsOwner = true }
                },
                OtherUsers = new List<MemberDto>
                {
                    new MemberDto { Id = Guid.NewGuid(), Email = "other@example.com", IsOwner = false }
                }
            };

            _orgRepoMock.Setup(r => r.GetListUsersByOrganizationId(orgId)).ReturnsAsync(expected);

            var result = await _orgService.GetListUsersByOrganizationId(orgId);

            Assert.NotNull(result);
            Assert.Single(result.Members);
            Assert.Single(result.OtherUsers);
            Assert.Equal("member@example.com", result.Members[0].Email);
            Assert.True(result.Members[0].IsOwner);
            Assert.Equal("other@example.com", result.OtherUsers[0].Email);
            Assert.False(result.OtherUsers[0].IsOwner);
        }

        [Fact]
        public async Task GetListUsersByOrganizationId_NotFound_ShouldReturnNull()
        {
            var orgId = Guid.NewGuid();
            _orgRepoMock.Setup(r => r.GetListUsersByOrganizationId(orgId)).ReturnsAsync((OrganizationUsersDto?)null);

            var result = await _orgService.GetListUsersByOrganizationId(orgId);

            Assert.Null(result);
        }
    }
}
