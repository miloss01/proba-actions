using DockerHubBackend.Models;
using System.Text.Json;

namespace DockerHubBackend.Dto.Response
{
    public class BaseUserDTO
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string? Location { get; set; }
        public string Username { get; set; }        

        public BaseUserDTO() { }

        public BaseUserDTO(BaseUser user)
        {
            Id = user.Id.ToString();
            Email = user.Email;
            Location = user.Location;
            Username = user.Username;
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
