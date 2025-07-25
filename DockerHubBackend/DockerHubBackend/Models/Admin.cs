namespace DockerHubBackend.Models
{
    public class Admin : BaseUser
    {
        public Admin() { }

        public Admin(string email, string username, string password, string location)
        {
            Email = email;
            Username = username;
            Password = password;
            Location = location;
            Badge = Badge.NoBadge;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
