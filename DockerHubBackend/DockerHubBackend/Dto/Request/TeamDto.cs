using System.Diagnostics.CodeAnalysis;
using DockerHubBackend.Dto.Response;
using DockerHubBackend.Dto.Response.Organization;
using DockerHubBackend.Models;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace DockerHubBackend.Dto.Request
{
    public class TeamDto
    {
        public Guid Id {  get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }

        public ICollection<MemberDto>? Members { get; set; }
        public Guid OrganizationId { get; set; }

        [SetsRequiredMembers]
        public TeamDto()
        {

        }

        [SetsRequiredMembers]
        public TeamDto(string name, string desription, ICollection<MemberDto> members, Guid organizationId)
        {
            Name = name;
            Members = members;
            Description = desription;
            OrganizationId = organizationId;
        }

        [SetsRequiredMembers]
        public TeamDto(string name, string desription, ICollection<MemberDto> members)
        {
            Name = name;
            Members = members;
            Description = desription;
        }

        [SetsRequiredMembers]
        public TeamDto(Guid id, string name, string desription, ICollection<MemberDto> members)
        {
            Id = id;
            Name = name;
            Members = members;
            Description = desription;
        }

        [SetsRequiredMembers]
        public TeamDto(Team team)
        {
            ICollection<MemberDto> memberDtos = new HashSet<MemberDto>();
            foreach (StandardUser user in team.Members) memberDtos.Add(new MemberDto { Id = user.Id, Email = user.Email, IsOwner = false  });
            Id = team.Id;
            Name = team.Name;
            Members = memberDtos;
            Description = team.Description;
        }
        public Team ToTeam(Organization organization)
        {
            return new Team
            {
                Name = Name,
                Description = Description,
                OrganizationId = OrganizationId,
                Organization = organization
            };
        }
    }
}
