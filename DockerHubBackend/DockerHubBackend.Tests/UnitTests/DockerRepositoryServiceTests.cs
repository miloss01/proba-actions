using DockerHubBackend.Dto.Request;
using DockerHubBackend.Dto.Response;
using DockerHubBackend.Dto.Response.Organization;
using DockerHubBackend.Exceptions;
using DockerHubBackend.Models;
using DockerHubBackend.Repository.Interface;
using DockerHubBackend.Repository.Utils;
using DockerHubBackend.Services.Implementation;
using DockerHubBackend.Services.Interface;
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
    public class DockerRepositoryServiceTests
    {
        private readonly Mock<IDockerRepositoryRepository> _mockDockerRepositoryRepository;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IOrganizationRepository> _mockOrganizationRepository;
		private readonly Mock<IRegistryService> _mockRegistryService;

		private readonly DockerRepositoryService _service;
        private readonly Mock<ILogger<DockerRepositoryService>> _mockLogger = new Mock<ILogger<DockerRepositoryService>>();
		private readonly Mock<IUnitOfWork> _mockUnitOfWork;

        public DockerRepositoryServiceTests()
        {
            _mockDockerRepositoryRepository = new Mock<IDockerRepositoryRepository>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockOrganizationRepository = new Mock<IOrganizationRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockUnitOfWork.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(Mock.Of<IDbContextTransaction>());
            _mockRegistryService = new Mock<IRegistryService>();
            _service = new DockerRepositoryService(_mockDockerRepositoryRepository.Object, _mockUserRepository.Object, _mockOrganizationRepository.Object, _mockLogger.Object, _mockRegistryService.Object, _mockUnitOfWork.Object);
        }

        [Fact]
        public void GetDockerRepositoryById_ProvideIdThatExists_ReturnsProvidedDockerRepository()
        {
            var dockerRepository = new DockerRepository { Id = Guid.NewGuid(), Name = "repo1" };

            _mockDockerRepositoryRepository.Setup(dockerRepositoryRepository => dockerRepositoryRepository.GetFullDockerRepositoryById(dockerRepository.Id)).Returns(dockerRepository);

            var result = _service.GetDockerRepositoryById(dockerRepository.Id);

            Assert.Equal(dockerRepository, result);
        }

        [Fact]
        public void GetDockerRepositoryById_ProvideIdThatDoesNotExist_ThrowsNotFoundException()
        {
            DockerRepository dockerRepository = null;
            Guid id = Guid.NewGuid();

            _mockDockerRepositoryRepository.Setup(dockerRepositoryRepository => dockerRepositoryRepository.GetFullDockerRepositoryById(It.IsAny<Guid>())).Returns(dockerRepository);

            var exception = Assert.Throws<NotFoundException>(() => _service.GetDockerRepositoryById(id));

            Assert.Equal($"Docker repository with id {id.ToString()} not found.", exception.Message);
        }

		[Fact]
		public async Task GetRepository_RepositoryExists_ReturnsRepository()
		{
			// Arrange
			var repositoryId = Guid.NewGuid();
			DockerRepository expectedRepository = new DockerRepository { Id = repositoryId, Name = ""};
			_mockDockerRepositoryRepository
				.Setup(repo => repo.GetDockerRepositoryById(repositoryId))
				.ReturnsAsync(expectedRepository);

			// Act
			var result = await _service.getRepository(repositoryId);

			// Assert
			Assert.NotNull(result);
			Assert.Equal(expectedRepository, result);
			_mockDockerRepositoryRepository.Verify(repo => repo.GetDockerRepositoryById(repositoryId), Times.Once);
		}

		[Fact]
		public async Task GetRepository_RepositoryDoesNotExist_ThrowsNotFoundException()
		{
			// Arrange
			var repositoryId = Guid.NewGuid();
			_mockDockerRepositoryRepository
				.Setup(repo => repo.GetDockerRepositoryById(repositoryId))
				.ReturnsAsync((DockerRepository?)null);

			// Act & Assert
			var exception = await Assert.ThrowsAsync<NotFoundException>(() => _service.getRepository(repositoryId));
			Assert.Equal($"Docker repository with id {repositoryId} not found.", exception.Message);
			_mockDockerRepositoryRepository.Verify(repo => repo.GetDockerRepositoryById(repositoryId), Times.Once);
		}

		[Fact]
		public async Task ChangeDockerRepositoryDescription_ValidRepository_UpdatesDescription()
		{
			// Arrange
			var repositoryId = Guid.NewGuid();
			var initialDescription = "Old Description";
			var newDescription = "New Description";

			var repository = new DockerRepository
			{
				Id = repositoryId,
				Name = "",
				Description = initialDescription
			};

			_mockDockerRepositoryRepository
				.Setup(repo => repo.GetDockerRepositoryById(repositoryId))
				.ReturnsAsync(repository);

			_mockDockerRepositoryRepository
				.Setup(repo => repo.Update(It.IsAny<DockerRepository>()))
				.ReturnsAsync((DockerRepository repository) => repository);

			// Act
			var result = await _service.ChangeDockerRepositoryDescription(repositoryId, newDescription);

			// Assert
			Assert.NotNull(result);
			Assert.Equal(repositoryId.ToString(), result.Id);
			Assert.Equal(newDescription, result.Description);
			_mockDockerRepositoryRepository.Verify(repo => repo.GetDockerRepositoryById(repositoryId), Times.Once);
			_mockDockerRepositoryRepository.Verify(repo => repo.Update(repository), Times.Once);
		}

		[Fact]
		public async Task ChangeDockerRepositoryVisibilyty_ValidRepository_UpdatesVisibility()
		{
			// Arrange
			var repositoryId = Guid.NewGuid();
			var initialVisibility = true;
			var newVisibility = false;

			var repository = new DockerRepository
			{
				Id = repositoryId,
				Name = "",
				IsPublic = initialVisibility
			};

			_mockDockerRepositoryRepository
				.Setup(repo => repo.GetDockerRepositoryById(repositoryId))
				.ReturnsAsync(repository);

			_mockDockerRepositoryRepository
				.Setup(repo => repo.Update(It.IsAny<DockerRepository>()))
				.ReturnsAsync((DockerRepository repository) => repository);

			// Act
			var result = await _service.ChangeDockerRepositoryVisibility(repositoryId, newVisibility);

			// Assert
			Assert.NotNull(result);
			Assert.Equal(repositoryId.ToString(), result.Id);
			Assert.Equal(newVisibility, result.IsPublic);
			_mockDockerRepositoryRepository.Verify(repo => repo.GetDockerRepositoryById(repositoryId), Times.Once);
			_mockDockerRepositoryRepository.Verify(repo => repo.Update(repository), Times.Once);
		}

		[Fact]
		public async Task CreateDockerRepository_UserOwner_ReturnsRepositoryDTO()
		{
			var ownerGuid = Guid.NewGuid();

			// Arrange
			var createDto = new CreateRepositoryDto
			{
				Name = "TestRepo",
				Description = "TestDescription",
				IsPublic = true,
				Owner = ownerGuid.ToString()
			};

			var user = new StandardUser { Id = ownerGuid, Username = "testUser", Email= "lala", Password = ""};
			Organization? organization = null;

			_mockUserRepository
				.Setup(svc => svc.GetUserById(ownerGuid))
				.ReturnsAsync(user);
			_mockOrganizationRepository
				.Setup(svc => svc.GetOrganizationById(ownerGuid))
				.ReturnsAsync(organization);
			_mockDockerRepositoryRepository
				.Setup(repo => repo.Create(It.IsAny<DockerRepository>()))
				.ReturnsAsync((DockerRepository repository) => repository);

			// Act
			var result = await _service.CreateDockerRepository(createDto);

			// Assert
			Assert.NotNull(result);
			Assert.Equal(createDto.Name, result.Name);
			Assert.Equal(createDto.Description, result.Description);
			Assert.Equal(createDto.IsPublic, result.IsPublic);
			Assert.Equal(user.Email, result.Owner);

			_mockUserRepository.Verify(svc => svc.GetUserById(ownerGuid), Times.Once);
			_mockOrganizationRepository.Verify(svc => svc.GetOrganizationById(ownerGuid), Times.Once);
			_mockDockerRepositoryRepository.Verify(repo => repo.Create(It.IsAny<DockerRepository>()), Times.Once);
		}

		[Fact]
		public async Task CreateDockerRepository_OrganizationOwner_ReturnsRepositoryDTO()
		{
			var ownerGuid = Guid.NewGuid();

			// Arrange
			var createDto = new CreateRepositoryDto
			{
				Name = "TestRepo",
				Description = "TestDescription",
				IsPublic = true,
				Owner = ownerGuid.ToString()
			};

			var organization = new Organization { Id = Guid.NewGuid(), Name = "testOrg", ImageLocation = "", OwnerId = ownerGuid };
			StandardUser? user = null;

			_mockUserRepository
				.Setup(svc => svc.GetUserById(ownerGuid))
				.ReturnsAsync(user);
			_mockOrganizationRepository
				.Setup(svc => svc.GetOrganizationById(ownerGuid))
				.ReturnsAsync(organization);
			_mockDockerRepositoryRepository
				.Setup(repo => repo.Create(It.IsAny<DockerRepository>()))
				.ReturnsAsync((DockerRepository repository) => repository);

			// Act
			var result = await _service.CreateDockerRepository(createDto);

			// Assert
			Assert.NotNull(result);
			Assert.Equal(createDto.Name, result.Name);
			Assert.Equal(createDto.Description, result.Description);
			Assert.Equal(createDto.IsPublic, result.IsPublic);
			Assert.Equal(organization.Name, result.Owner);

			_mockUserRepository.Verify(svc => svc.GetUserById(ownerGuid), Times.Once);
			_mockOrganizationRepository.Verify(svc => svc.GetOrganizationById(ownerGuid), Times.Once);
			_mockDockerRepositoryRepository.Verify(repo => repo.Create(It.IsAny<DockerRepository>()), Times.Once);
		}

		[Fact]
		public async Task DeleteDockerRepository_RepositoryExists_CallsDeleteMethod()
		{
			// Arrange
			var repositoryId = Guid.NewGuid();
			DockerRepository? rep = null;
			var repository = new DockerRepository { Id = repositoryId, Name = "TestRepo" };

			_mockDockerRepositoryRepository
				.Setup(repo => repo.GetDockerRepositoryByIdWithImages(repositoryId))
				.ReturnsAsync(repository);

			_mockDockerRepositoryRepository
				.Setup(repo => repo.Delete(repositoryId))
				.ReturnsAsync(rep);

			// Act
			await _service.DeleteDockerRepository(repositoryId);

			// Assert
			_mockDockerRepositoryRepository.Verify(repo => repo.GetDockerRepositoryByIdWithImages(repositoryId), Times.Once);
			_mockDockerRepositoryRepository.Verify(repo => repo.Delete(repositoryId), Times.Once);
		}

		[Fact]
		public async Task DeleteDockerRepository_RepositoryDoesNotExist_ThrowsNotFoundException()
		{
			// Arrange
			var repositoryId = Guid.NewGuid();
			DockerRepository? rep = null;

			_mockDockerRepositoryRepository
				.Setup(repo => repo.GetDockerRepositoryById(repositoryId))
				.ReturnsAsync(rep);

			// Act & Assert
			var exception = await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteDockerRepository(repositoryId));
			Assert.Equal($"Docker repository with id {repositoryId} not found.", exception.Message);

			_mockDockerRepositoryRepository.Verify(repo => repo.GetDockerRepositoryByIdWithImages(repositoryId), Times.Once);
			_mockDockerRepositoryRepository.Verify(repo => repo.Delete(It.IsAny<Guid>()), Times.Never);
		}

		[Fact]
		public async Task GetRepositoriesByUserId_UserExists_ReturnsRepositories()
		{
			// Arrange
			var userId = Guid.NewGuid();
			var user = new StandardUser { Id = userId, Email = "test@example.com", Password = "", Username = "" };
			var userRepositories = new List<DockerRepository>
				{
					new DockerRepository { Id = Guid.NewGuid(), Name = "Repo1" },
					new DockerRepository { Id = Guid.NewGuid(), Name = "Repo2" }
				};
			var organizations = new List<OrganizationOwnershipDto>
				{
					new OrganizationOwnershipDto { Id = Guid.NewGuid(), Name = "Org1" }
				};
			var organizationRepositories = new List<DockerRepository>
				{
					new DockerRepository { Id = Guid.NewGuid(), Name = "OrgRepo1" }
				};

			_mockUserRepository
				.Setup(repo => repo.GetUserById(userId))
				.ReturnsAsync(user);

			_mockDockerRepositoryRepository
				.Setup(repo => repo.GetRepositoriesByUserOwnerId(userId))
				.ReturnsAsync(userRepositories);

			_mockOrganizationRepository
				.Setup(repo => repo.GetUserOrganizations(user.Email))
				.ReturnsAsync(organizations);

			_mockDockerRepositoryRepository
				.Setup(repo => repo.GetRepositoriesByOrganizationOwnerId(It.IsAny<Guid>()))
				.ReturnsAsync(organizationRepositories);

			// Act
			var result = await _service.GetRepositoriesByUserId(userId);

			// Assert
			Assert.NotNull(result);
			Assert.Equal(3, result.Count);
			Assert.Contains(result, r => r.Name == "Repo1");
			Assert.Contains(result, r => r.Name == "Repo2");
			Assert.Contains(result, r => r.Name == "OrgRepo1");

			_mockUserRepository.Verify(repo => repo.GetUserById(userId), Times.Once);
			_mockDockerRepositoryRepository.Verify(repo => repo.GetRepositoriesByUserOwnerId(userId), Times.Once);
			_mockOrganizationRepository.Verify(repo => repo.GetUserOrganizations(user.Email), Times.Once);
			_mockDockerRepositoryRepository.Verify(repo => repo.GetRepositoriesByOrganizationOwnerId(It.IsAny<Guid>()), Times.Once);
		}

		[Fact]
		public async Task GetRepositoriesByUserId_UserDoesNotExist_ThrowsNotFoundException()
		{
			// Arrange
			var userId = Guid.NewGuid();
			StandardUser? user =  null;


			_mockUserRepository
				.Setup(repo => repo.GetUserById(userId))
				.ReturnsAsync(user);

			// Act & Assert
			var exception = await Assert.ThrowsAsync<NotFoundException>(() => _service.GetRepositoriesByUserId(userId));
			Assert.Equal($"User with id {userId} not found.", exception.Message);

			_mockUserRepository.Verify(repo => repo.GetUserById(userId), Times.Once);
			_mockDockerRepositoryRepository.Verify(repo => repo.GetRepositoriesByUserOwnerId(It.IsAny<Guid>()), Times.Never);
			_mockOrganizationRepository.Verify(repo => repo.GetUserOrganizations(It.IsAny<string>()), Times.Never);
		}


	}

}

