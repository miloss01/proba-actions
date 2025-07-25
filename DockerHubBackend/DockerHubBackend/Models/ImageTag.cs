using System.ComponentModel.DataAnnotations.Schema;

namespace DockerHubBackend.Models
{
    public class ImageTag : BaseEntity
    {
        public Guid DockerImageId { get; set; }
        public required string Name { get; set; }
    }
}
