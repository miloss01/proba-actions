using DockerHubBackend.Data;
using DockerHubBackend.Dto.Response;
using DockerHubBackend.Models;
using DockerHubBackend.Repository.Interface;
using DockerHubBackend.Repository.Utils;
using Microsoft.EntityFrameworkCore;

namespace DockerHubBackend.Repository.Implementation
{
    public class DockerImageRepository : CrudRepository<DockerImage>, IDockerImageRepository
    {
        public DockerImageRepository(DataContext context) : base(context) { }

        public PageDTO<DockerImage> GetDockerImages(int page, int pageSize, string? searchTerm, string? badges)
        {
            searchTerm = searchTerm ?? string.Empty;
            badges = badges ?? string.Empty;

            var badgeList = badges.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(badge => Enum.TryParse<Badge>(badge, true, out _))
                .Select(badge => Enum.Parse<Badge>(badge, true))
                .ToList();

            var dockerImages = _context.DockerImages
                .AsQueryable()
                .Include(img => img.Repository)
                    .ThenInclude(repo => repo.UserOwner)
                .Include(img => img.Repository)
                    .ThenInclude(repo => repo.OrganizationOwner)
                .Include(img => img.Tags)
                .Where(img => !img.IsDeleted)
                .Where(img => img.Repository.IsPublic)
                .Where(img => !badgeList.Any() ||
                              badgeList.Contains(img.Repository.Badge))
                .Where(img => img.Repository.Name.Contains(searchTerm) ||
                              img.Id.ToString().Contains(searchTerm))
                .ToList();

            var pageDto = new PageDTO<DockerImage>(
                            dockerImages
                             .OrderByDescending(img => img.Repository.Badge == Badge.DockerOfficialImage)
                             .ThenByDescending(img => img.Repository.Badge == Badge.VerifiedPublisher)
                             .ThenByDescending(img => img.Repository.Badge == Badge.SponsoredOSS)
                             .ThenByDescending(img => img.Repository.Badge == Badge.NoBadge)
                             .ThenByDescending(img => img.Repository.StarCount)
                             .Skip((page - 1) * pageSize)
                             .Take(pageSize)
                             .ToList(),
                            dockerImages.Count
                        );

            return pageDto;
        }

		public async Task<DockerImage?> GetDockerImageById(Guid id)
		{
			return await _context.DockerImages.FirstOrDefaultAsync(repo => repo.Id == id);
		}

        public async Task<DockerImage?> GetDockerImageByIdWithRepository(Guid id)
        {
            return await _context.DockerImages.Include(image => image.Repository).FirstOrDefaultAsync(image => image.Id == id);
        }
    }
}
