using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace DockerHubBackend.Dto.Request
{
    public class ChangePasswordDto
    {
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
        [MaxLength(20, ErrorMessage = "Password must be at most 20 characters long")]
        public string NewPassword { get; set; }
        public string Token { get; set; }

        public ChangePasswordDto() { }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
