using DockerHubBackend.Dto.Request;
using DockerHubBackend.Dto.Response;
using DockerHubBackend.Exceptions;
using DockerHubBackend.Repository.Interface;
using DockerHubBackend.Security;
using DockerHubBackend.Services.Implementation;
using DockerHubBackend.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System.IdentityModel.Tokens.Jwt;

namespace DockerHubBackend.Controllers
{
    [Route("api/dockerImages")]
    [ApiController]
    public class DockerImageController : ControllerBase
    {
        private readonly IDockerImageService _dockerImageService;

        public DockerImageController(IDockerImageService dockerImageService)
        {
            _dockerImageService = dockerImageService;
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult GetDockerImages(int page, int pageSize, string? searchTerm, string? badges)
        {
            var pagedDockerImages = _dockerImageService.GetDockerImages(page, pageSize, searchTerm, badges);
            var pageDTO = new PageDTO<DockerImageDTO>(
                                pagedDockerImages.Data.Select(img => new DockerImageDTO
                                {
                                    RepositoryName = img.Repository.Name,
                                    RepositoryId = img.Repository.Id.ToString(),
                                    Badge = img.Repository.Badge.ToString(),
                                    Description = img.Repository.Description,
                                    CreatedAt = img.CreatedAt.ToString(),
                                    LastPush = img.LastPush != null ? img.LastPush.ToString() : null,
                                    ImageId = img.Id.ToString(),
                                    StarCount = img.Repository.StarCount,
                                    Tags = img.Tags.Select(tag => tag.Name).ToList(),
                                    Digest = img.Digest,
                                    Owner = img.Repository.OrganizationOwner == null ? img.Repository.UserOwner.Email : img.Repository.OrganizationOwner.Name
                                }).ToList(),
                                pagedDockerImages.TotalNumberOfElements
                            );

            return Ok(pageDTO);
        }

		[HttpDelete("delete/{id}")]
		public async Task<IActionResult> DeleteImageById(string id)
		{
			var parsed = Guid.TryParse(id, out var repositoryId);

			if (!parsed)
			{
				throw new NotFoundException("Image not found. Bad image id.");
			}

			try
			{
				await _dockerImageService.DeleteDockerImage(repositoryId);
				return Ok(new { message = "Image deleted successfully." });
			}
			catch (NotFoundException ex)
			{
				return NotFound(new { error = ex.Message });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { error = ex.Message });
			}
		}
	}
}
