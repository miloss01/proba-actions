using System.ComponentModel;
using DockerHubBackend.Dto.Request;
using DockerHubBackend.Dto.Response;
using DockerHubBackend.Dto.Response.Organization;
using DockerHubBackend.Exceptions;
using DockerHubBackend.Models;
using DockerHubBackend.Repository.Interface;
using DockerHubBackend.Services.Interface;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.EntityFrameworkCore;

namespace DockerHubBackend.Services.Implementation
{
    public class TeamService : ITeamService
    {   
        private readonly ITeamRepository _repository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IUserRepository _userRepository;
        private readonly IDockerRepositoryRepository _dockerRepositoryRepository;
        private readonly ILogger<TeamService> _logger;

        public TeamService(ITeamRepository repository, IOrganizationRepository organizationRepository,
            IUserRepository userRepository, IDockerRepositoryRepository dockerRepositoryRepository, ILogger<TeamService> logger)
        {
            _repository = repository;
            _organizationRepository = organizationRepository;
            _userRepository = userRepository;
            _dockerRepositoryRepository = dockerRepositoryRepository;
            _logger = logger;
        }

        public async Task<ICollection<TeamDto>?> GetTeams(Guid organizationId)
        {
            _logger.LogInformation("Fetching teams for organization with ID {OrganizationId}", organizationId);
            return await _repository.GetByOrganizationId(organizationId);
        }

        public async Task<TeamDto?> Get(Guid id)
        {
            _logger.LogInformation("Fetching team with ID {TeamId}", id);
            Team? team = await _repository.Get(id);
            if (team == null) 
            {
                _logger.LogError("Team with ID {TeamId} not found", id);
                throw new NotFoundException("Team not found."); 
            }
            return new TeamDto(team);
        }

        public async Task<TeamDto> Create(TeamDto teamDto)
        {
            _logger.LogInformation("Creating team with name {TeamName} for organization {OrganizationId}", teamDto.Name, teamDto.OrganizationId);
            Organization? organization = await _organizationRepository.Get(teamDto.OrganizationId);
            if (organization == null) 
            {
                _logger.LogError("Organization with ID {OrganizationId} does not exist", teamDto.OrganizationId);
                throw new NotFoundException("Organization does not exist."); 
            }

            Team? t = await _repository.GetByOrgIdAndTeamName(organization.Id, teamDto.Name);
            if (t != null)
            {
                _logger.LogError("Team with name {TeamName} already exists in organization {OrganizationId}", teamDto.Name, teamDto.OrganizationId);
                throw new BadRequestException("Team with chosen name already exists.");
            }

            Team team = teamDto.ToTeam(organization);
            team.Members = await toStandardUsers(teamDto.Members);
            await _repository.Create(team);
            
            Team returnedTeam = await _repository.GetByName(teamDto.Name);
            ICollection<MemberDto> memberDtos = new HashSet<MemberDto>();
            foreach (StandardUser user in team.Members) { memberDtos.Add(user.ToMemberDto()); }

            _logger.LogInformation("Successfully created team {TeamName} with ID {TeamId}", returnedTeam.Name, returnedTeam.Id);
            return new TeamDto(returnedTeam.Id, returnedTeam.Name, returnedTeam.Description, memberDtos);       
        }

        public async Task<TeamDto> AddMembers(Guid teamId, ICollection<MemberDto> memberDtos)
        {
            _logger.LogInformation("Adding members to team with ID {TeamId}", teamId);
            Team? team = await _repository.Get(teamId);
            if (team == null) 
            {
                _logger.LogError("Team with ID {TeamId} does not exist", teamId);
                throw new NotFoundException("Team does not exist."); 
            }
            team.Members = await toStandardUsers(memberDtos);
            Team? updatedTeam = await _repository.Update(team);
            if (updatedTeam == null) 
            {
                _logger.LogError("Error occurred while adding members to team with ID {TeamId}", teamId);
                throw new BadRequestException("Error occured while adding members. Addition aborted."); 
            }

            _logger.LogInformation("Successfully added members to team with ID {TeamId}", teamId);
            return new TeamDto(updatedTeam);
        }

        public async Task<TeamPermissionResponseDto> AddPermissions(TeamPermissionRequestDto teamPermissionDto)
        {
            _logger.LogInformation("Adding permissions for team {TeamId} on repository {RepositoryId}", teamPermissionDto.TeamId, teamPermissionDto.RepositoryId);
            TeamPermission? teamPerm = _repository.GetTeamPermission(teamPermissionDto.RepositoryId, teamPermissionDto.TeamId);
            if (teamPerm != null) 
            {
                _logger.LogError("Permission already exists for team {TeamId} on repository {RepositoryId}", teamPermissionDto.TeamId, teamPermissionDto.RepositoryId);
                throw new BadRequestException("Team-Permission already exists."); 
            }

            DockerRepository? dr = await _dockerRepositoryRepository.Get(teamPermissionDto.RepositoryId);
            if (dr == null) 
            {
                _logger.LogError("Repository with ID {RepositoryId} not found", teamPermissionDto.RepositoryId);
                throw new NotFoundException("Repositoy not found."); 
            }

            Team? t = await _repository.Get(teamPermissionDto.TeamId);
            if (t == null) {
                _logger.LogError("Team with ID {TeamId} not found", teamPermissionDto.TeamId);
                throw new NotFoundException("Team not found."); 
            }
            TeamPermission tp = new TeamPermission
            {
                TeamId = teamPermissionDto.TeamId,
                RepositoryId = teamPermissionDto.RepositoryId,
                Team = t,
                Repository = dr,
                Permission = toPermissionType(teamPermissionDto.Permission),
            };
            _repository.AddPermissions(tp);

            _logger.LogInformation("Successfully added permission for team {TeamId} on repository {RepositoryId}", teamPermissionDto.TeamId, teamPermissionDto.RepositoryId);
            return new TeamPermissionResponseDto(tp);
        }

        public async Task<TeamDto> Update(TeamDto teamDto, Guid id)
        {
            _logger.LogInformation("Updating team with ID {TeamId}", id);
            Team? team = await _repository.Get(id);
            if (team == null) 
            {
                _logger.LogError("Team with ID {TeamId} not found", id);
                throw new NotFoundException("Team not found."); 
            }
            team.Name = teamDto.Name;
            team.Description = teamDto.Description;
            team = await _repository.Update(team);
            if (team == null) 
            {
                _logger.LogError("Failed to update team with ID {TeamId}", id);
                throw new NotFoundException("Team not found. Update aborted."); 
            }

            _logger.LogInformation("Successfully updated team with ID {TeamId}", id);
            return new TeamDto(team);
        }

        public async Task<TeamDto> Delete(Guid id)
        {
            _logger.LogInformation("Deleting team with ID {TeamId}", id);
            Team? team = await _repository.Delete(id);
            if (team == null) 
            {
                _logger.LogError("Team with ID {TeamId} not found", id);
                throw new NotFoundException("Team not found."); 
            }

            _logger.LogInformation("Successfully deleted team with ID {TeamId}", id);
            return new TeamDto(team);
        }

        public async Task<ICollection<TeamPermission>> GetTeamPermissions(Guid id)
        {
            _logger.LogInformation("Fetching permissions for team with ID {TeamId}", id);
            return await _repository.GetTeamPermissions(id);
        }

        private async Task<ICollection<StandardUser>> toStandardUsers(ICollection<MemberDto> memberDtos)
        {
            _logger.LogInformation("Converting email DTOs to standard users");
            ICollection<StandardUser?> members = new HashSet<StandardUser?>();
            foreach (MemberDto memberDto in memberDtos)
            {
                BaseUser? baseUser = await _userRepository.GetUserByEmail(memberDto.Email);
                StandardUser user = (StandardUser)baseUser;
                members.Add(user);
            }
            return members;
        }

        private PermissionType toPermissionType(string input)
        {
            if (Enum.TryParse<PermissionType>(input, true, out PermissionType permissionType))
            {
                return permissionType;
            }
            else
            {
                _logger.LogError("Permission type {PermissionType} not found", input);
                throw new NotFoundException("Chosen permission type not found.");
            }
        }

    }
}
