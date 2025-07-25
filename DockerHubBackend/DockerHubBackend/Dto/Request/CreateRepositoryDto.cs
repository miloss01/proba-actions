using System.ComponentModel.DataAnnotations;

namespace DockerHubBackend.Dto.Request
{
	public class CreateRepositoryDto
	{
		[Required(ErrorMessage = "Name is required.")]
		[StringLength(50, ErrorMessage = "Name must be less than 50 characters.")]
		public required string Name { get; set; }

		public required string Description { get; set; }

		[Required(ErrorMessage = "Owner is required.")]
		public required string Owner { get; set; }

		[Required(ErrorMessage = "Visibility is required.")]
		public required bool IsPublic { get; set; }

		public CreateRepositoryDto()
		{
		}

		public CreateRepositoryDto(string name, string description, string owner, bool isPublic)
		{
			Name = name;
			Description = description;
			Owner = owner;
			IsPublic = isPublic;
		}

		public override string? ToString()
		{
			return $"CreateRepositoryDto {{ Name = {Name}, Description = {Description}, Owner = {Owner}, IsPublic = {IsPublic} }}";
		}
	}
}
