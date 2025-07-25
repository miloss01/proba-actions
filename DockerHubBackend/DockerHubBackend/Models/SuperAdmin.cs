namespace DockerHubBackend.Models
{
    public class SuperAdmin : BaseUser
    {
        public required bool IsVerified { get; set; }
    }
}
