using System.ComponentModel.DataAnnotations;

namespace DockerHubBackend.Dto.Request
{
	public class UpdateRepositoryVisibilityDto
	{
		[Required(ErrorMessage = "Name is required.")]
		public required string RepositoryId { get; set; }

		[Required(ErrorMessage = "Visibility is required.")]
		public required bool isPublic { get; set; }
	}
}
