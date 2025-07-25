using System.Collections.ObjectModel;
using DockerHubBackend.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DockerHubBackend.Models;
using DockerHubBackend.Dto.Response;
using DockerHubBackend.Dto.Request;
using DockerHubBackend.Dto.Response.Organization;

namespace DockerHubBackend.Controllers
{
    [Route("api/team")]
    [ApiController]
    public class TeamController : ControllerBase
    {

        private readonly ITeamService _teamService;
        private readonly IConfiguration _configuration;

        public TeamController(ITeamService teamService, IConfiguration configuration)
        {
            _teamService = teamService;
            _configuration = configuration;
        }

        [HttpGet("org/{organizationId}")]
        public async Task<IActionResult> GetTeamsByOrganizationId([FromRoute] Guid organizationId)
        {
            ICollection<TeamDto>? teams = await _teamService.GetTeams(organizationId);
            if (teams == null) { return NotFound("Teams not found."); }
            return Ok(teams);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TeamDto teamDto)
        {
            TeamDto team = await _teamService.Create(teamDto);
            return Ok(team);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            TeamDto? team = await _teamService.Get(id);
            if (team == null) { return NotFound("Team not found."); }
            return Ok(team);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromBody] TeamDto teamDto, [FromRoute] Guid id)
        {
            TeamDto team = await _teamService.Update(teamDto, id);
            return Ok(team);
        }


        [HttpPut("member/{id}")]
        public async Task<IActionResult> UpdateMembers([FromBody] ICollection<MemberDto> memberDtos, [FromRoute] Guid id)
        {
            var result = await _teamService.AddMembers(id, memberDtos);
            return Ok(result);
        }

        [HttpPost("permission")]
        public async Task<IActionResult> AddPermission([FromBody] TeamPermissionRequestDto tpRequestDto)
        {
            var result = await _teamService.AddPermissions(tpRequestDto);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            TeamDto team = await _teamService.Delete(id);
            return Ok(team);
        }

        [HttpGet("repositories/{id}")]
        public async Task<IActionResult> GetRepositories([FromRoute] Guid id)
        {
            ICollection<TeamPermission> res = await _teamService.GetTeamPermissions(id);
            return Ok(res);
        }
    }
}
