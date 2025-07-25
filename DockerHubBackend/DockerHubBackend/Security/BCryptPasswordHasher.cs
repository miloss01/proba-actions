using Microsoft.AspNetCore.Identity;
using BCrypt.Net;

namespace DockerHubBackend.Security
{
    public class BCryptPasswordHasher : IPasswordHasher<string>
    {
        public string HashPassword(string user, string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public PasswordVerificationResult VerifyHashedPassword(string user, string hashedPassword, string providedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword)
                ? PasswordVerificationResult.Success
                : PasswordVerificationResult.Failed;
        }
    }
}
