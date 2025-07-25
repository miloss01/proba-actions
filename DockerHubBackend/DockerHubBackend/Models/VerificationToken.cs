using System.ComponentModel.DataAnnotations.Schema;

namespace DockerHubBackend.Models
{
    public class VerificationToken : BaseEntity
    {
        public required string Token { get; set; }

        public required DateTime ValidUntil { get; set; }
        
        public required Guid UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public required BaseUser User { get; set; }
    }
}
