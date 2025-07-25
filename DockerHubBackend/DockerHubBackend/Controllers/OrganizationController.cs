using DockerHubBackend.Data;
using DockerHubBackend.Dto.Request;
using DockerHubBackend.Dto.Response.Organization;
using DockerHubBackend.Exceptions;
using DockerHubBackend.Models;
using DockerHubBackend.Repository.Interface;
using DockerHubBackend.Security;
using DockerHubBackend.Services.Implementation;
using DockerHubBackend.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using System.IdentityModel.Tokens.Jwt;

namespace DockerHubBackend.Controllers
{
    [Route("api/organization")]
    [ApiController]
    public class OrganizationController : ControllerBase
    {
        private readonly IOrganizationService _orgService;

        public OrganizationController(IOrganizationService organizationService)
        {
            _orgService = organizationService;
        }

        [HttpPost]
        public async Task<IActionResult> AddOrganization([FromBody] AddOrganizationDto dto)
        {
            if (dto == null)
            {
                return BadRequest("Invalid dto");
            }
            var addedOrganization = await _orgService.AddOrganization(dto);
            if(addedOrganization == null)
            {
                return BadRequest("Error database saving");
            }

            return Ok(addedOrganization);
        }

        [HttpGet("{email}")]
        public async Task<IActionResult> GetUserOrganizations(string email)
        {
            try
            {
                var organizations = await _orgService.GetOrganizations(email);

                if (organizations == null || !organizations.Any())
                {
                    return NotFound("User is not a member or owner of any organization.");
                }

                return Ok(organizations);  
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{organizationId}/members")]
        public async Task<ActionResult<OrganizationUsersDto>> GetMembers(Guid organizationId)
        {
            var users = await _orgService.GetListUsersByOrganizationId(organizationId);

            if (users == null)
            {
                return NotFound("No members.");
            }

            return Ok(users);
        }

        [HttpPost("add-member")]
        public async Task<IActionResult> AddMemberToOrganization([FromBody] AddMemberDto request)
        {
            var result = await _orgService.AddMemberToOrganization(request.OrganizationId, request.UserId);

            if (result == "Organization not found.")
            {
                return NotFound(result);
            }

            if (result == "User not found.")
            {
                return NotFound(result);
            }

            return Ok();
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteOrganization(Guid id)
        {
            try
            {
                await _orgService.DeleteOrganization(id);
                return Ok(new { message = "Organization deleted successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
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

        [HttpPut("update")]
        public async Task<IActionResult> UpdateOrganization([FromBody] UpdateOrganizationDto updateDto)
        {
            if (updateDto == null)
            {
                return BadRequest("Invalid data.");
            }

            try
            {
                await _orgService.UpdateOrganization(updateDto.Id, updateDto.ImageLocation, updateDto.Description);
                return Ok(new { message = "Organization updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
