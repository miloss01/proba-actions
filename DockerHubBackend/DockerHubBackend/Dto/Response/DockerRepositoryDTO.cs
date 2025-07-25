using DockerHubBackend.Models;
using System.Text.Json;

namespace DockerHubBackend.Dto.Response
{
    public class DockerRepositoryDTO
    {

		/*public DockerRepositoryDTO(DockerRepository repository)
		{
			
		}*/

		public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsPublic { get; set; }
        public int StarCount { get; set; }
        public string Badge { get; set; }
        public ICollection<DockerImageDTO> Images { get; set; }
        public string Owner { get; set; }
        public string CreatedAt { get; set; }

		public DockerRepositoryDTO() { }

		public DockerRepositoryDTO(DockerRepository dockerRepository)
		{
			Id = dockerRepository.Id.ToString();
			Name = dockerRepository.Name;
			Description = dockerRepository.Description;
			Badge = dockerRepository.Badge.ToString();
			CreatedAt = dockerRepository.CreatedAt.ToString();
			IsPublic = dockerRepository.IsPublic;
			StarCount = dockerRepository.StarCount;
			Owner = dockerRepository.OrganizationOwner == null
				? (dockerRepository.UserOwner == null ? null : dockerRepository.UserOwner.Email)
				: dockerRepository.OrganizationOwner.Name;
			Images = dockerRepository.Images.Select(img => new DockerImageDTO
			{
				RepositoryName = img.Repository.Name,
				RepositoryId = img.Repository.Id.ToString(),
				Badge = img.Repository.Badge.ToString(),
				Description = img.Repository.Description,
				CreatedAt = img.CreatedAt.ToString(),
				LastPush = img.LastPush != null ? img.LastPush.ToString() : null,
				ImageId = img.Id.ToString(),
				Tags = img.Tags.Select(tag => tag.Name).ToList(),
				Digest = img.Digest,
				StarCount = img.Repository.StarCount,
				Owner = img.Repository.OrganizationOwner == null ? img.Repository.UserOwner.Email : img.Repository.OrganizationOwner.Name
			}).ToList();

		}

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}