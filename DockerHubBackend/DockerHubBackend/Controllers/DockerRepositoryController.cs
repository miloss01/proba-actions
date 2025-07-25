using DockerHubBackend.Dto.Request;
using DockerHubBackend.Dto.Response;
using DockerHubBackend.Exceptions;
using DockerHubBackend.Models;
using DockerHubBackend.Repository.Interface;
using DockerHubBackend.Security;
using DockerHubBackend.Services.Implementation;
using DockerHubBackend.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System.IdentityModel.Tokens.Jwt;

namespace DockerHubBackend.Controllers
{
    [Route("api/dockerRepositories")]
    [ApiController]
    public class DockerRepositoryController : ControllerBase
    {
        private readonly IDockerRepositoryService _dockerRepositoryService;

        public DockerRepositoryController(IDockerRepositoryService dockerRepositoryService)
        {
            _dockerRepositoryService = dockerRepositoryService;
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult GetDockerRepositories(int page, int pageSize, string? searchTerm, string? badges)
        {
            var pagedDockerRepositories = _dockerRepositoryService.GetDockerRepositories(page, pageSize, searchTerm, badges);
            var pageDTO = new PageDTO<DockerRepositoryDTO>(
                                pagedDockerRepositories.Data.Select(repo => new DockerRepositoryDTO(repo)).ToList(),
                                pagedDockerRepositories.TotalNumberOfElements
                            );

            return Ok(pageDTO);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public ActionResult GetDockerRepositoryById(string id)
        {
            var parsed = Guid.TryParse(id, out var repositoryId);

            if (!parsed)
            {
                throw new NotFoundException("Repository not found. Bad repository id.");
            }

            var dockerRepository = _dockerRepositoryService.GetDockerRepositoryById(repositoryId);

            var dockerRepositoryDto = new DockerRepositoryDTO(dockerRepository);

            return Ok(dockerRepositoryDto);
        }

		[HttpGet("all/{id}")]
		public async Task<IActionResult> GetAllUserRepositories(string id)
		{
			var parsed = Guid.TryParse(id, out var userId);

			if (!parsed)
			{
				throw new NotFoundException("User not found. Bad User id.");
			}
			try
			{
				var repositories = await _dockerRepositoryService.GetRepositoriesByUserId(userId);
				return Ok(repositories);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { Message = "An error occurred while getting the repositories.", Details = ex.Message });
			}
		}


		[HttpPost]
		public async Task<IActionResult> CreateRepository([FromBody] CreateRepositoryDto dto)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var responce = await _dockerRepositoryService.CreateDockerRepository(dto);
			return Ok(responce);
		}

		[HttpPut("update-description")]
		public async Task<IActionResult> UpdateRepositoryDescription([FromBody] UpdateRepositoryDescriptionDto dto)
		{
			var parsed = Guid.TryParse(dto.RepositoryId, out var repositoryId);

			if (!parsed)
			{
				throw new NotFoundException("Repository not found. Bad repository id.");
			}
			try
			{
				var updatedRepository = await _dockerRepositoryService.ChangeDockerRepositoryDescription(repositoryId, dto.NewDescription);
				return Ok(updatedRepository);
			}
			catch (NotFoundException ex)
			{
				return NotFound(new { Message = ex.Message });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { Message = "An error occurred while updating the repository description.", Details = ex.Message });
			}
		}

		[HttpPut("update-visibility")]
		public async Task<IActionResult> UpdateRepositoryVisibility([FromBody] UpdateRepositoryVisibilityDto dto)
		{
			var parsed = Guid.TryParse(dto.RepositoryId, out var repositoryId);

			if (!parsed)
			{
				throw new NotFoundException("Repository not found. Bad repository id.");
			}
			try
			{
				var updatedRepository = await _dockerRepositoryService.ChangeDockerRepositoryVisibility(repositoryId, dto.isPublic);
				return Ok(updatedRepository);
			}
			catch (NotFoundException ex)
			{
				return NotFound(new { Message = ex.Message });
			}
			catch (BadRequestException ex)
			{
				return BadRequest(ex.Message);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { Message = "An error occurred while updating the repository visibility.", Details = ex.Message });
			}
		}

		[HttpDelete("delete/{id}")]
		public async Task<IActionResult> DeleteRepositoryById(string id)
		{
			var parsed = Guid.TryParse(id, out var repositoryId);

			if (!parsed)
			{
				throw new NotFoundException("Repository not found. Bad repository id.");
			}

			try
			{
				await _dockerRepositoryService.DeleteDockerRepository(repositoryId);
				return Ok(new { message = "Repository deleted successfully." });
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

        [HttpGet("star/{id}")]
        [AllowAnonymous]
        public IActionResult GetStarRepositoriesForUser(string id)
        {
            var parsed = Guid.TryParse(id, out var userId);

            if (!parsed)
            {
                throw new NotFoundException("User not found. Bad user id.");
            }

            var starRepositories = _dockerRepositoryService.GetStarRepositoriesForUser(userId);
            var starRepositoriesDto = starRepositories.Select(starRepository => new DockerRepositoryDTO(starRepository));

            return Ok(starRepositoriesDto);
        }

        [HttpGet("private/{id}")]
        [AllowAnonymous]
        public IActionResult GetPrivateRepositoriesForUser(string id)
        {
            var parsed = Guid.TryParse(id, out var userId);

            if (!parsed)
            {
                throw new NotFoundException("User not found. Bad user id.");
            }

            var privateRepositories = _dockerRepositoryService.GetPrivateRepositoriesForUser(userId);
            var privateRepositoriesDto = privateRepositories.Select(privateRepository => new DockerRepositoryDTO(privateRepository));

            return Ok(privateRepositoriesDto);
        }

        [HttpPatch("star/{userId}/{repositoryId}")]
        [AllowAnonymous]
        public IActionResult AddStarRepository(string userId, string repositoryId)
        {
            var parsed = Guid.TryParse(userId, out var userGuid);

            if (!parsed)
            {
                throw new NotFoundException("User not found. Bad user id.");
            }

            parsed = Guid.TryParse(repositoryId, out var repositoryGuid);

            if (!parsed)
            {
                throw new NotFoundException("Repository not found. Bad repository id.");
            }

            _dockerRepositoryService.AddStarRepository(userGuid, repositoryGuid);

            return NoContent();
        }

        [HttpPatch("star/remove/{userId}/{repositoryId}")]
        [AllowAnonymous]
        public IActionResult RemoveStarRepository(string userId, string repositoryId)
        {
            var parsed = Guid.TryParse(userId, out var userGuid);

            if (!parsed)
            {
                throw new NotFoundException("User not found. Bad user id.");
            }

            parsed = Guid.TryParse(repositoryId, out var repositoryGuid);

            if (!parsed)
            {
                throw new NotFoundException("Repository not found. Bad repository id.");
            }

            _dockerRepositoryService.RemoveStarRepository(userGuid, repositoryGuid);

            return NoContent();
        }

        [HttpGet("star/notallowed/{id}")]
        [AllowAnonymous]
        public IActionResult GetNotAllowedRepositoriesToStarForUser(string id)
        {
            var parsed = Guid.TryParse(id, out var userId);

            if (!parsed)
            {
                throw new NotFoundException("User not found. Bad user id.");
            }

            var myRepositories = _dockerRepositoryService.GetAllRepositoriesForUser(userId);
            var organizationRepositories = _dockerRepositoryService.GetOrganizationRepositoriesForUser(userId);
            
            var allGuids = myRepositories
                .Concat(organizationRepositories)
                .Select(repository => repository.Id.ToString());

            return Ok(allGuids);
        }
    }
}
