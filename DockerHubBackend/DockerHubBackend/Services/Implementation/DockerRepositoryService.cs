using DockerHubBackend.Data;
using DockerHubBackend.Dto.Request;
using DockerHubBackend.Dto.Response;
using DockerHubBackend.Dto.Response.Organization;
using DockerHubBackend.Exceptions;
using DockerHubBackend.Models;
using DockerHubBackend.Repository.Implementation;
using DockerHubBackend.Repository.Interface;
using DockerHubBackend.Repository.Utils;
using DockerHubBackend.Security;
using DockerHubBackend.Services.Interface;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;

namespace DockerHubBackend.Services.Implementation
{
	public class DockerRepositoryService : IDockerRepositoryService
	{
		private readonly IDockerRepositoryRepository _dockerRepositoryRepository;
		private readonly IUserRepository _userRepository;
		private readonly IOrganizationRepository _organizationRepository;
        private readonly IRegistryService _registryService;
        private readonly ILogger<DockerRepositoryService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public DockerRepositoryService(IDockerRepositoryRepository dockerRepositoryRepository, IUserRepository userRepository, IOrganizationRepository organizationRepository, ILogger<DockerRepositoryService> logger, IRegistryService registryService, IUnitOfWork unitOfWork)
        {
            _dockerRepositoryRepository = dockerRepositoryRepository;
            _userRepository = userRepository;
            _organizationRepository = organizationRepository;
            _logger = logger;
            _registryService = registryService;
            _unitOfWork = unitOfWork;
        }

        public async Task<DockerRepository?> getRepository(Guid id)
		{
			_logger.LogInformation("Fetching Docker repository with ID: {Id}", id);

			var repository = await _dockerRepositoryRepository.GetDockerRepositoryById(id);
			if (repository == null)
			{
				_logger.LogError("Docker repository with ID: {Id} not found.", id);
				throw new NotFoundException($"Docker repository with id {id.ToString()} not found.");
			}

			_logger.LogInformation("Docker repository with ID: {Id} successfully retrieved.", id);
			return repository;
		}

        public async Task<DockerRepository> getRepositoryWithImages(Guid id)
        {
            _logger.LogInformation("Fetching Docker repository with ID: {Id}", id);

            var repository = await _dockerRepositoryRepository.GetDockerRepositoryByIdWithImages(id);
            if (repository == null)
            {
                _logger.LogError("Docker repository with ID: {Id} not found.", id);
                throw new NotFoundException($"Docker repository with id {id.ToString()} not found.");
            }

            _logger.LogInformation("Docker repository with ID: {Id} successfully retrieved.", id);
            return repository;
        }

        public async Task<DockerRepositoryDTO> ChangeDockerRepositoryDescription(Guid id, string description)
		{
			_logger.LogInformation("Changing description for Docker repository with ID: {Id}", id);

			DockerRepository? repository = await getRepository(id);

			Console.WriteLine(repository.UserOwner);

			repository.Description = description;

			// Save changes to the repository
			await _dockerRepositoryRepository.Update(repository);

			// Map the updated repository to a DTO (assuming a mapping method or library)
			var updatedRepositoryDto = new DockerRepositoryDTO(repository);

			_logger.LogInformation("Successfully updated description for Docker repository with ID: {Id}", id);

			// Return the updated DTO
			return updatedRepositoryDto;
		}


		public async Task<DockerRepositoryDTO> ChangeDockerRepositoryVisibility(Guid id, bool visibility)
		{
			_logger.LogInformation("Changing visibility for Docker repository with ID: {Id}", id);

			DockerRepository? repository = await getRepository(id);

			if (repository.IsPublic == visibility)
			{
				_logger.LogError("The visibility for Docker repository with ID: {Id} is already set to {Visibility}.", id, visibility);
				throw new BadRequestException("The new visibility value is the same as the current value.");
			}
			repository.IsPublic = visibility;

			// Save changes to the repository
			await _dockerRepositoryRepository.Update(repository);

			_logger.LogInformation("Successfully changed visibility for Docker repository with ID: {Id}", id);

			// Map the updated repository to a DTO (assuming a mapping method or library)
			var updatedRepositoryDto = new DockerRepositoryDTO(repository);

			// Return the updated DTO
			return updatedRepositoryDto;
		}

		public async Task<BaseUser?> getUser(string id)
		{
			// Check the correctnes of id
			var parsed = Guid.TryParse(id, out var userId);
			if (!parsed)
			{
				return null;
			}

			// Convert the user to standard user
			try
			{
				var user = await _userRepository.GetUserById(userId);
				return (BaseUser?)user;
			}
			catch (Exception)
			{
				return null;
			}
		}


		public async Task<Organization?> getOrganization(string id)
		{
			// Check the correctnes of id
			var parsed = Guid.TryParse(id, out var orgId);
			if (!parsed)
			{
				return null;
			}

			return await _organizationRepository.GetOrganizationById(orgId);
		}

		public async Task<DockerRepositoryDTO> CreateDockerRepository(CreateRepositoryDto createRepositoryDto)
		{
			_logger.LogInformation("Creating a new Docker repository: {RepositoryName}", createRepositoryDto.Name);

			// Get either User or Organization
			var userOwner = await getUser(createRepositoryDto.Owner);
			var organizationOwner = await getOrganization(createRepositoryDto.Owner);
			if ((userOwner == null) && (organizationOwner == null))
			{
				_logger.LogError("Invalid owner name: {NamespaceName}. Must be either a username or organization name.", createRepositoryDto.Owner);
				throw new ArgumentException("Invalid namespace name. It can be either an organization name or username.");
			}
			var badge = Badge.NoBadge;
			if (userOwner != null && userOwner.GetType().Name != "StandardUser")
			{
				badge = Badge.VerifiedPublisher;
			}
			// Create the repo
			var newRepository = new DockerRepository
			{
				Name = createRepositoryDto.Name,
				Description = createRepositoryDto.Description,
				IsPublic = createRepositoryDto.IsPublic,
				StarCount = 0,
				Badge = badge,
				Images = new HashSet<DockerImage>(),
				Teams = new HashSet<Team>(),
				UserOwner = userOwner,
				OrganizationOwner = organizationOwner
			};
			await _dockerRepositoryRepository.Create(newRepository);
			_logger.LogInformation("Successfully created Docker repository: {RepositoryName}", createRepositoryDto.Name);

			// Map the saved entity to a DTO
			var repositoryDto = new DockerRepositoryDTO(newRepository);

			// Return the created DTO
			return repositoryDto;
		}

		public async Task DeleteDockerRepository(Guid id)
		{
			_logger.LogInformation("Deleting Docker repository with ID: {Id}", id);


            await using var tx = await _unitOfWork.BeginTransactionAsync();

            try
            {
                DockerRepository repository = await getRepositoryWithImages(id);

                await _dockerRepositoryRepository.Delete(id);

                foreach (var image in repository.Images)
                {
                    await _registryService.DeleteDockerImage(image.Digest, repository.Name);
                }
            }
			catch (NotFoundException)
            {
                await tx.RollbackAsync();
                throw;
			}
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _logger.LogError(ex, "Failed to delete Docker repository with ID: {Id}", id);
                throw new Exception("Something went wrong");
            }
            
			_logger.LogInformation("Successfully deleted Docker repository with ID: {Id}", id);
		}

		public DockerRepository GetDockerRepositoryById(Guid id)
		{
			_logger.LogInformation("Fetching Docker repository with ID: {RepositoryId}", id);
			var dockerRepository = _dockerRepositoryRepository.GetFullDockerRepositoryById(id);

			if (dockerRepository == null)
			{
				_logger.LogWarning("Docker repository with ID: {RepositoryId} not found.", id);
				throw new NotFoundException($"Docker repository with id {id.ToString()} not found.");
			}

			_logger.LogInformation("Successfully fetched Docker repository with ID: {RepositoryId}", id);
			return dockerRepository;
		}

		public async Task<List<DockerRepositoryDTO>> GetRepositoriesByUserId(Guid id)
		{
			_logger.LogInformation("Fetching repositories for user with ID: {UserId}", id);

			// get user
			var user = await _userRepository.GetUserById(id);
			if (user == null)
			{
				_logger.LogWarning("User with ID: {UserId} not found.", id);
				throw new NotFoundException($"User with id {id.ToString()} not found.");
			}

			List<DockerRepositoryDTO> responce = new List<DockerRepositoryDTO>();

			// get direct user repos
			var repositories = await _dockerRepositoryRepository.GetRepositoriesByUserOwnerId(id);
			if (repositories != null)
			{
				_logger.LogInformation("Found {RepositoryCount} repositories directly owned by user with ID: {UserId}", repositories.Count, id);
				responce = repositories.Select(repo => new DockerRepositoryDTO(repo)).ToList();
			}

			// get repos for each organization he is in
			var organizations = await _organizationRepository.GetUserOrganizations(user.Email);
			if (organizations != null)
			{
				_logger.LogInformation("User with ID: {UserId} belongs to {OrganizationCount} organizations. Fetching organization repositories.", id, organizations.Count);
				await AddOrganizationRepositories(responce, organizations);
			}

			_logger.LogInformation("Successfully fetched all repositories for user with ID: {UserId}", id);
			return responce;
		}

		private async Task AddOrganizationRepositories(List<DockerRepositoryDTO> responce, List<OrganizationOwnershipDto> organizations)
		{
			foreach (var organization in organizations)
			{
				_logger.LogInformation("Fetching repositories for organization with ID: {OrganizationId} and Name: {OrganizationName}", organization.Id, organization.Name);
				var repositories = await _dockerRepositoryRepository.GetRepositoriesByOrganizationOwnerId(organization.Id);
				if (repositories != null)
				{
					foreach (var repo in repositories)
					{
						var repoDto = new DockerRepositoryDTO(repo);
						repoDto.Owner = organization.Name;
						responce.Add(repoDto);
					}
					_logger.LogInformation("Added {RepositoryCount} repositories for organization with ID: {OrganizationId}", repositories.Count, organization.Id);
				}

			}

		}
		public List<DockerRepository> GetStarRepositoriesForUser(Guid userId)
		{
			_logger.LogInformation("Fetching starred repositories for user with ID: {UserId}", userId);
			var starredRepos = _dockerRepositoryRepository.GetStarRepositoriesForUser(userId);
			_logger.LogInformation("Found {StarredRepositoryCount} starred repositories for user with ID: {UserId}", starredRepos.Count, userId);
			return starredRepos;
		}

		public List<DockerRepository> GetPrivateRepositoriesForUser(Guid userId)
		{
			_logger.LogInformation("Fetching private repositories for user with ID: {UserId}", userId);
			var privateRepos = _dockerRepositoryRepository.GetPrivateRepositoriesForUser(userId);
			_logger.LogInformation("Found {PrivateRepositoryCount} private repositories for user with ID: {UserId}", privateRepos.Count, userId);
			return privateRepos;
		}

		public List<DockerRepository> GetOrganizationRepositoriesForUser(Guid userId)
		{
			_logger.LogInformation("Fetching organization repositories for user with ID: {UserId}", userId);
			var orgRepos = _dockerRepositoryRepository.GetOrganizationRepositoriesForUser(userId);
			_logger.LogInformation("Found {OrganizationRepositoryCount} organization repositories for user with ID: {UserId}", orgRepos.Count, userId);
			return orgRepos;
		}

		public List<DockerRepository> GetAllRepositoriesForUser(Guid userId)
		{
			_logger.LogInformation("Fetching all repositories for user with ID: {UserId}", userId);
			var allRepos = _dockerRepositoryRepository.GetAllRepositoriesForUser(userId);
			_logger.LogInformation("Found {TotalRepositoryCount} repositories for user with ID: {UserId}", allRepos.Count, userId);
			return allRepos;
		}

		public void AddStarRepository(Guid userId, Guid repositoryId)
		{
			_logger.LogInformation("Adding repository with ID: {RepositoryId} to starred list for user with ID: {UserId}", repositoryId, userId);
			_dockerRepositoryRepository.AddStarRepository(userId, repositoryId);
			_logger.LogInformation("Successfully added repository with ID: {RepositoryId} to starred list for user with ID: {UserId}", repositoryId, userId);
		}

		public void RemoveStarRepository(Guid userId, Guid repositoryId)
		{
			_logger.LogInformation("Removing repository with ID: {RepositoryId} from starred list for user with ID: {UserId}", repositoryId, userId);
			_dockerRepositoryRepository.RemoveStarRepository(userId, repositoryId);
			_logger.LogInformation("Successfully removed repository with ID: {RepositoryId} from starred list for user with ID: {UserId}", repositoryId, userId);
		}

        public PageDTO<DockerRepository> GetDockerRepositories(int page, int pageSize, string? searchTerm, string? badges)
        {
            _logger.LogInformation("Fetching Docker repositories with parameters: Page={Page}, PageSize={PageSize}, SearchTerm={SearchTerm}, Badges={Badges}", page, pageSize, searchTerm, badges);
            var result = _dockerRepositoryRepository.GetDockerRepositories(page, pageSize, searchTerm, badges);

            _logger.LogInformation("Fetched Docker repositories.");
            return result;
        }
    }
}