using DockerHubBackend.Data;
using DockerHubBackend.Models;
using DockerHubBackend.Repository.Interface;
using DockerHubBackend.Repository.Utils;
using Microsoft.EntityFrameworkCore;

namespace DockerHubBackend.Repository.Implementation
{
    public class VerificationTokenRepository : CrudRepository<VerificationToken>, IVerificationTokenRepository
    {
        public VerificationTokenRepository(DataContext context) : base(context) { }

        public async Task<VerificationToken?> GetTokenByUserId(Guid userId)
        {
            return await _context.VerificationTokens.FirstOrDefaultAsync(vt => vt.UserId == userId);
        }

        public async Task<VerificationToken?> GetTokenByValue(string token)
        {
            return await _context.VerificationTokens.Include(vt => vt.User).FirstOrDefaultAsync(vt => vt.Token == token);
        }
    }
}
