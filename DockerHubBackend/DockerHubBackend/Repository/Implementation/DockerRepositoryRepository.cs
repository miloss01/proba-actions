using DockerHubBackend.Data;
using DockerHubBackend.Dto.Response;
using DockerHubBackend.Exceptions;
using DockerHubBackend.Models;
using DockerHubBackend.Repository.Interface;
using DockerHubBackend.Repository.Utils;
using Microsoft.EntityFrameworkCore;

namespace DockerHubBackend.Repository.Implementation
{
    public class DockerRepositoryRepository : CrudRepository<DockerRepository>, IDockerRepositoryRepository
    {
        public DockerRepositoryRepository(DataContext context) : base(context) { }

		public async Task<DockerRepository?> GetDockerRepositoryById(Guid id)
		{
			return await _context.DockerRepositories.FirstOrDefaultAsync(repo => repo.Id == id);
		}

        public async Task<List<DockerRepository>?> GetRepositoriesByUserOwnerId(Guid id)
        { 
            return await _context.DockerRepositories
				                 .Where(repo => repo.UserOwnerId == id)
						         .ToListAsync();
		}

		public DockerRepository GetFullDockerRepositoryById(Guid id)
        {
            return _context.DockerRepositories
                .AsQueryable()
                .Include(dockerRepository => dockerRepository.UserOwner)
                .Include(dockerRepository => dockerRepository.OrganizationOwner)
                .Include(dockerRepository => dockerRepository.Images)
                    .ThenInclude(img => img.Tags)
                .FirstOrDefault(dockerRepository => dockerRepository.Id == id);
        }

		public async Task<List<DockerRepository>?> GetRepositoriesByOrganizationOwnerId(Guid id)
		{
			return await _context.DockerRepositories
								 .Where(repo => repo.OrganizationOwnerId == id)
								 .ToListAsync();
		}
        public List<DockerRepository> GetStarRepositoriesForUser(Guid userId)
        {
            return _context.Users
                .OfType<StandardUser>()
                .Where(user => !user.IsDeleted)
                .AsQueryable()
                .Include(user => user.StarredRepositories)
                    .ThenInclude(starRepository => starRepository.UserOwner)
                .Include(user => user.StarredRepositories)
                    .ThenInclude(starRepository => starRepository.OrganizationOwner)
                .Include(user => user.StarredRepositories)
                    .ThenInclude(starRepository => starRepository.Images)
                .First(user => user.Id == userId)
                .StarredRepositories
                .ToList();
        }

        public List<DockerRepository> GetPrivateRepositoriesForUser(Guid userId)
        {
            return _context.DockerRepositories
                .AsQueryable()
                .Include(dockerRepository => dockerRepository.UserOwner)
                .Include(dockerRepository => dockerRepository.OrganizationOwner)
                    .ThenInclude(organization => organization.Owner)
                .Include(dockerRepository => dockerRepository.Images)
                .Where(dockerRepository => !dockerRepository.IsDeleted)
                .Where(dockerRepository => dockerRepository.UserOwnerId == userId ||
                                           dockerRepository.OrganizationOwner.OwnerId == userId)
                .Where(dockerRepository => !dockerRepository.IsPublic)
                .ToList();
        }

        public List<DockerRepository> GetOrganizationRepositoriesForUser(Guid userId)
        {
            return _context.Organizations
                .Include(organization => organization.Members)
                .Include(organization => organization.Repositories)
                .Include(organization => organization.Owner)
                .Where(organization => organization.Members.Any(member => member.Id == userId) ||
                                       organization.Owner.Id == userId)
                .SelectMany(organization => organization.Repositories)
                .ToList();
        }

        public List<DockerRepository> GetAllRepositoriesForUser(Guid userId)
        {
            return _context.Users
                .OfType<StandardUser>()
                .Include(user => user.MyRepositories)
                .First(user => user.Id == userId)
                .MyRepositories
                .ToList();
        }

        public void AddStarRepository(Guid userId, Guid repositoryId)
        {
            var user = _context.Users.OfType<StandardUser>().FirstOrDefault(user => user.Id == userId);
            var repository = _context.DockerRepositories.Find(repositoryId);
            user.StarredRepositories.Add(repository);
            repository.StarCount += 1;
            _context.SaveChanges();
        }

        public void RemoveStarRepository(Guid userId, Guid repositoryId)
        {
            var user = _context.Users
                .OfType<StandardUser>()
                .Include(user => user.StarredRepositories)
                .FirstOrDefault(user => user.Id == userId);
            var repository = _context.DockerRepositories.Find(repositoryId);
            user.StarredRepositories.Remove(repository);
            repository.StarCount -= 1;
            _context.SaveChanges();
        }

        public PageDTO<DockerRepository> GetDockerRepositories(int page, int pageSize, string? searchTerm, string? badges)
        {
            searchTerm = searchTerm ?? string.Empty;
            badges = badges ?? string.Empty;

            var badgeList = badges.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(badge => Enum.TryParse<Badge>(badge, true, out _))
                .Select(badge => Enum.Parse<Badge>(badge, true))
                .ToList();

            var dockerRepositories = _context.DockerRepositories
                .AsQueryable()
                .Include(repo => repo.UserOwner)
                .Include(repo => repo.OrganizationOwner)
                .Include(repo => repo.Images)
                    .ThenInclude(img => img.Tags)
                .Where(repo => !repo.IsDeleted)
                .Where(repo => repo.IsPublic)
                .Where(repo => !badgeList.Any() ||
                              badgeList.Contains(repo.Badge))
                .Where(repo => repo.Name.Contains(searchTerm) ||
                              repo.Id.ToString().Contains(searchTerm))
                .ToList();

            var pageDto = new PageDTO<DockerRepository>(
                            dockerRepositories
                             .OrderByDescending(repo => repo.Badge == Badge.DockerOfficialImage)
                             .ThenByDescending(repo => repo.Badge == Badge.VerifiedPublisher)
                             .ThenByDescending(repo => repo.Badge == Badge.SponsoredOSS)
                             .ThenByDescending(repo => repo.Badge == Badge.NoBadge)
                             .ThenByDescending(repo => repo.StarCount)
                             .Skip((page - 1) * pageSize)
                             .Take(pageSize)
                             .ToList(),
                            dockerRepositories.Count
                        );

            return pageDto;
        }

        public async Task<DockerRepository?> GetDockerRepositoryByIdWithImages(Guid id)
        {
            return await _context.DockerRepositories.Include(repo => repo.Images).FirstOrDefaultAsync(repo => repo.Id == id);
        }
    }
}
