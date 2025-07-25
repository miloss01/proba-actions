using DockerHubBackend.Dto.Response.Organization;

namespace DockerHubBackend.Models
{
    public class StandardUser : BaseUser
    {
        
        public ICollection<Organization> MyOrganizations { get; set; } = new HashSet<Organization>();
        public ICollection<Organization> MemberOrganizations { get; set; } = new HashSet<Organization>();
        public ICollection<Team> Teams { get; set; } = new HashSet<Team>();

        public StandardUser() { }

        public StandardUser(string email, string username, string password, string location)
        {
            Email = email;
            Username = username;
            Password = password;
            Location = location;
            Badge = Badge.NoBadge;
            CreatedAt = DateTime.UtcNow;
        }

        public MemberDto ToMemberDto()
        {
            return new MemberDto { Email = Email };
        }

    }
}
