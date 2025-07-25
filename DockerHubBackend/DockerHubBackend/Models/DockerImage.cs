using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace DockerHubBackend.Models
{
    public class DockerImage : BaseEntity
    {
        public required Guid DockerRepositoryId { get; set; }

        [ForeignKey(nameof(DockerRepositoryId))]
        public required DockerRepository Repository { get; set; }
        public DateTime? LastPush { get; set; }
        public required string Digest { get; set; }
        public ICollection<ImageTag> Tags { get; set; } = new HashSet<ImageTag>();
    }
}
