using System.ComponentModel.DataAnnotations;

namespace DockerHubBackend.Dto.Request
{
	public class UpdateRepositoryDescriptionDto
	{
		[Required(ErrorMessage = "Name is required.")]
		public required string RepositoryId { get; set; }
		public string NewDescription { get; set; } = string.Empty;
	}
}
