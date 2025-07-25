using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using DockerHubBackend.Data;
using DockerHubBackend.Dto.Request;
using DockerHubBackend.Dto.Response;
using DockerHubBackend.Dto.Response.Organization;
using DockerHubBackend.Models;
using DockerHubBackend.Repository.Interface;
using DockerHubBackend.Repository.Utils;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace DockerHubBackend.Repository.Implementation
{
    public class TeamRepository : CrudRepository<Team>, ITeamRepository
    {

        public TeamRepository(DataContext context) : base(context) { }

        public async Task<ICollection<TeamDto>> GetByOrganizationId(Guid organizationId)
        {
            var teams = await _context.Teams
                .Where(team => team.OrganizationId == organizationId)
                .Include(team => team.Members)
                .Select(team => new TeamDto
                (
                    team.Id,
                    team.Name,
                    team.Description,
                    team.Members.Select(member => new MemberDto
                    {
                        Email = member.Email,
                    }).ToList()
                ))
                .ToListAsync();
            return new Collection<TeamDto>(teams);

        }

        override public async Task<Team?> Get(Guid id)
        {
            var team = await _context.Teams
                .Where(team => team.Id == id)
                .Include(team => team.Members)
                .FirstOrDefaultAsync();
            return team;
        }

        public async Task<Team> GetByName(string name)
        {
            return await _context.Teams.FirstOrDefaultAsync(team => team.Name == name);
        }

        public async Task<ICollection<StandardUser>> GetMembers(Guid id)
        {
            Team? team = await _context.Teams
                .Where(t => t.Id == id)
                .Include(t => t.Members)
                .FirstOrDefaultAsync();

            if (team == null)
            {
                throw new KeyNotFoundException($"Team with ID {id} not found.");
            }

            return team.Members.ToList();
        }

        public void AddPermissions(TeamPermission teamPermission)
        {
            _context.TeamPermissions.Add(teamPermission);
            _context.SaveChangesAsync();
        }

        public TeamPermission? GetTeamPermission(Guid repositoryId, Guid id)
        {
            return _context.TeamPermissions
            .FirstOrDefault(tp => tp.RepositoryId == repositoryId && tp.TeamId == id);

        }

        public async Task<ICollection<TeamPermission>> GetTeamPermissions(Guid id)
        {
            return await _context.TeamPermissions.Where(tp => tp.TeamId == id).Include(tp => tp.Team).Include(tp => tp.Repository).ToListAsync();
        }

        public async Task<Team?> GetByOrgIdAndTeamName(Guid orgId, string teamName)
        {
            return await _context.Teams.Include(team => team.Organization)
                .FirstOrDefaultAsync(team => team.Name == teamName && team.Organization.Id == orgId);
        }
    }
}
