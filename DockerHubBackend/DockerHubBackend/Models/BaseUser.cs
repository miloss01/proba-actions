using System.ComponentModel.DataAnnotations.Schema;

namespace DockerHubBackend.Models
{
    public abstract class BaseUser : BaseEntity
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public DateTime? LastPasswordChangeDate { get; set; }
        public VerificationToken? VerificationToken { get; set; }
        public required string Username { get; set; }
        public string? Location { get; set; }
        public Badge Badge { get; set; }
		public ICollection<DockerRepository> StarredRepositories { get; set; } = new HashSet<DockerRepository>();
		public ICollection<DockerRepository> MyRepositories { get; set; } = new HashSet<DockerRepository>();
	}
}
