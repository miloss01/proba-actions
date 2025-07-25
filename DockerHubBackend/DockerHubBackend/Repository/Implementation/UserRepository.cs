using DockerHubBackend.Data;
using DockerHubBackend.Models;
using DockerHubBackend.Repository.Interface;
using DockerHubBackend.Repository.Utils;
using Microsoft.EntityFrameworkCore;

namespace DockerHubBackend.Repository.Implementation
{
    public class UserRepository : CrudRepository<BaseUser>, IUserRepository
    {
        public UserRepository(DataContext context) : base(context) {}

        public async override Task<BaseUser?> Get(Guid id)
        {
            return await _entities.Include(u => u.VerificationToken).FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<BaseUser?> GetUserByEmail(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(user => user.Email == email);
        }

        public async Task<BaseUser?> GetUserWithTokenByEmail(string email)
        {
            return await _context.Users.Include(u => u.VerificationToken).FirstOrDefaultAsync(user => user.Email == email);
        }

        public async Task<BaseUser?> GetUserByUsername(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(user => user.Username == username);
        }

        public async Task<BaseUser?> GetUserById(Guid id) 
        {
            return await _context.Users.FindAsync(id);

		}
        public List<StandardUser> GetAllStandardUsers()
        {
            return _context.Users
                .OfType<StandardUser>()
                .Where(user => !user.IsDeleted)
                .ToList();
        }

        public void ChangeUserBadge(Badge badge, Guid userId)
        {
            var user = _context.Users.SingleOrDefault(u => u.Id == userId);
            user.Badge = badge;
            _context.SaveChanges();

            var userRepositories = _context.DockerRepositories
                .Include(dockerRepository => dockerRepository.UserOwner)
                .Include(dockerRepository => dockerRepository.OrganizationOwner)
                    .ThenInclude(organization => organization.Owner)
                .Where(dockerRepository => dockerRepository.UserOwnerId == userId ||
                                           dockerRepository.OrganizationOwner.OwnerId == userId)
                .ToList();

            foreach (var repository in userRepositories)
            {
                repository.Badge = badge;
            }

            _context.SaveChanges();
        }
    }
}
