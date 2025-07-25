using System.Text;

namespace DockerHubBackend.Startup
{
    public interface IRandomTokenGenerator
    {

        public string GenerateRandomPassword(int length);

        public string GenerateVerificationToken(int length);
    }
}