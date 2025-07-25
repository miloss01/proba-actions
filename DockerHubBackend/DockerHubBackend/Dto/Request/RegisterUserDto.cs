using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace DockerHubBackend.Dto.Request
{
    public class RegisterUserDto
    {

        public string Email { get; set; }

        [MinLength(4, ErrorMessage = "Username must be at least 4 characters long")]
        [MaxLength(20, ErrorMessage = "Username must be at most 20 characters long")]
        public string Username { get; set; }

        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
        [MaxLength(20, ErrorMessage = "Password must be at most 20 characters long")]
        public string Password { get; set; }
        public string? Location { get; set; }

        public RegisterUserDto() { }
        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
