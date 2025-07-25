using DockerHubBackend.Dto.Request;
using DockerHubBackend.Dto.Response;
using DockerHubBackend.Exceptions;
using DockerHubBackend.Models;
using DockerHubBackend.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DockerHubBackend.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPatch("password/change")]
        [AllowAnonymous]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            await _userService.ChangePassword(changePasswordDto);
            return NoContent();
        }

        [HttpPost("")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto changePasswordDto)
        {
            var response = await _userService.Register<StandardUser>(changePasswordDto);
            return Ok(response);
        }

        [HttpPost("admin")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterUserDto userDto)
        { 
            var response = await _userService.Register<Admin>(userDto);
            return Ok(response);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetAllStandardUsers()
        {
            var standardUsers = _userService.GetAllStandardUsers();
            var standardUsersDtos = standardUsers.Select(standardUser => new MinifiedStandardUserDTO
            {
                Id = standardUser.Id.ToString(),
                Username = standardUser.Username,
                Badge = standardUser.Badge.ToString()
            });

            return Ok(standardUsersDtos);
        }

        [HttpPatch("{id}/badge/change")]
        [AllowAnonymous]
        public IActionResult ChangeUserBadge(string id, [FromBody] NewBadgeDTO newBadge)
        {
            var parsedId = Guid.TryParse(id, out var userId);

            if (!parsedId)
            {
                throw new NotFoundException("User not found. Bad user id.");
            }

            if (Enum.TryParse(typeof(Badge), newBadge.Badge, ignoreCase: true, out var parsedBadge))
            {
                var badge = (Badge)parsedBadge;
                _userService.ChangeUserBadge(badge, userId);
            }
            else
            {
                throw new BadRequestException("Badge not found. Bad badge variant.");
            }


            return NoContent();
        }
    }

}
