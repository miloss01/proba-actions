using Amazon.S3.Model;
using DockerHubBackend.Data;
using DockerHubBackend.Dto.Request;
using DockerHubBackend.Dto.Response.Organization;
using DockerHubBackend.Models;
using DockerHubBackend.Repository.Interface;
using DockerHubBackend.Repository.Utils;
using DockerHubBackend.Services.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DockerHubBackend.Repository.Implementation
{
    public class OrganizationRepository : CrudRepository<Organization>, IOrganizationRepository
    {
        private IUserRepository _userRepository;
        public OrganizationRepository(DataContext context, IUserRepository userRepository) : base(context) {
            this._userRepository = userRepository;
        }

        public async Task<Guid?> AddOrganization(AddOrganizationDto dto)
        {
            var user = await _userRepository.GetUserByEmail(dto.OwnerEmail);
            if (user == null) {
                return null;
            }

            var standardUser = user as StandardUser;
            if (standardUser == null)
            {
                return null;
            }

            var organization = new Organization
            {
                Name = dto.Name,
                Description = dto.Description,
                ImageLocation = dto.ImageLocation,
                OwnerId = user.Id,
                Owner = standardUser,
                Members = new HashSet<StandardUser>
                {
                    standardUser,
                }
            };

            _context.Organizations.Add(organization);
            await _context.SaveChangesAsync();
            return organization.Id;        
        }

        public async Task<List<OrganizationOwnershipDto>?> GetUserOrganizations(string email)
        {
            var user = await _userRepository.GetUserByEmail(email);
            if (user == null)
            {
                return null;
            }

            var organizations = await _context.Organizations
                .Where(o => !o.IsDeleted &&
                    (o.OwnerId == user.Id ||
                     o.Members.Any(m => m.Id == user.Id)))
                .Select(o => new OrganizationOwnershipDto
                {
                    Id = o.Id,
                    Name = o.Name,
                    Description = o.Description,
                    ImageLocation = o.ImageLocation,
                    CreatedAt = o.CreatedAt,
                    OwnerEmail = _context.Users
                                    .Where(u => u.Id == o.OwnerId)
                                    .Select(u => u.Email)
                                    .FirstOrDefault(),
                    IsOwner = o.OwnerId == user.Id 
                })
                .ToListAsync();

            return organizations;
        }

        public async Task<Organization?> GetOrganizationById(Guid id)
        {
            return await _context.Organizations.FindAsync(id);
        }

        public async Task<Organization?> GetOrganizationByIdWithRepositories(Guid id)
        {
            return await _context.Organizations.Include(o => o.Repositories).FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<OrganizationUsersDto> GetListUsersByOrganizationId(Guid organizationId)
        {
            var organization = await _context.Organizations
                .Where(o => o.Id == organizationId)
                .Include(o => o.Members) 
                .FirstOrDefaultAsync();

            if (organization == null)
            {
                return null;
            }

            var allUsers = await _context.Users.ToListAsync();

            var membersDto = organization.Members.Select(m => new MemberDto
            {
                Id = m.Id,
                Email = m.Email,
                IsOwner = m.MemberOrganizations.Any(o => o.OwnerId == m.Id)
            }).ToList();

            var otherUsersDto = allUsers
               .Where(u => !organization.Members.Any(m => m.Id == u.Id))
               .Select(u => new MemberDto
               {
                   Id = u.Id,
                   Email = u.Email,
                   IsOwner = false
               }).ToList();

            return new OrganizationUsersDto
            {
                Members = membersDto,
                OtherUsers = otherUsersDto
            };
        }

        public async Task<string> AddMemberToOrganization(Guid organizationId, Guid userId)
        {
            // check organization exist
            var organization = await _context.Organizations
                .Include(o => o.Members)
                .FirstOrDefaultAsync(o => o.Id == organizationId);
            if (organization == null)
            {
                return "Organization not found.";
            }

            // check user exist
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return "User not found.";
            }

            var standardUser = user as StandardUser;
            if (standardUser == null)
            {
                return "Not standard user.";
            }

            // add member
            if (!organization.Members.Contains(user))
            {
                organization.Members.Add(standardUser);
                await _context.SaveChangesAsync();
                return "User added to organization successfully.";
            }

            return "User is already a member of the organization.";
        }

        public async Task DeleteOrganization(Guid organizationId)
        {
            var organization = await _context.Organizations.FirstOrDefaultAsync(o => o.Id == organizationId);

            if (organization == null)
            {
                throw new KeyNotFoundException("Organization not found.");
            }

            organization.IsDeleted = true;

            await _context.SaveChangesAsync();
        }

        public async Task UpdateOrganization(Guid organizationId, string imageLocation, string description)
        {
            var organization = await _context.Organizations.FindAsync(organizationId);
            if (organization == null)
            {
                throw new Exception("Organization not found");
            }

            organization.ImageLocation = imageLocation;
            organization.Description = description;

            await _context.SaveChangesAsync();
        }
    }
}
