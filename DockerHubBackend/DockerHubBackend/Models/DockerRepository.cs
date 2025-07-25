using System.ComponentModel.DataAnnotations.Schema;

namespace DockerHubBackend.Models
{
    public class DockerRepository : BaseEntity
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public bool IsPublic { get; set; }
        public int StarCount { get; set; }
        public Badge Badge { get; set; }
        public ICollection<DockerImage> Images { get; set; } = new HashSet<DockerImage>();
        public ICollection<Team> Teams { get; set; } = new HashSet<Team>();
        public Guid? UserOwnerId { get; set; }
        [ForeignKey(nameof(UserOwnerId))]
        public BaseUser? UserOwner { get; set; }
        public Guid? OrganizationOwnerId { get; set; }
        [ForeignKey(nameof(OrganizationOwnerId))]
        public Organization? OrganizationOwner { get; set; }

		public override string? ToString()
		{
			return $"Name: {Name}, " +
		   $"Description: {Description ?? "N/A"}, " +
		   $"IsPublic: {IsPublic}, " +
		   $"StarCount: {StarCount}, " +
		   $"Badge: {Badge}, " +
		   /*$"Images: {string.Join(", ", Images.Select(img => img.ToString()))}, " +
		   $"Teams: {string.Join(", ", Teams.Select(team => team.ToString()))}, " +*/
		   $"UserOwnerId: {UserOwnerId?.ToString() ?? "N/A"}, " +
		   $"UserOwner: {UserOwner?.ToString() ?? "N/A"}, ";
		   /*$"OrganizationOwnerId: {OrganizationOwnerId?.ToString() ?? "N/A"}, " +
		   $"OrganizationOwner: {OrganizationOwner?.ToString() ?? "N/A"}";*/
		}
	}
}
