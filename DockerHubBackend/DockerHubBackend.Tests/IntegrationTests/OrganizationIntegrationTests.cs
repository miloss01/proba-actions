using DockerHubBackend.Dto.Request;
using DockerHubBackend.Dto.Response.Organization;
using DockerHubBackend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace DockerHubBackend.Tests.IntegrationTests
{
    public class OrganizationIntegrationTests : IntegrationTestBase
    {
        [Fact]
        public async Task AddOrganization_ShouldReturnOkAndId()
        {
            var ownerEmail = "user@email.com";
            var userId = Guid.NewGuid();

            await SeedDatabaseAsync(async db =>
            {
                var user = new StandardUser
                {
                    Id = userId,
                    Email = ownerEmail,
                    Username = "user",
                    Password = "pass"
                };
                db.Users.Add(user);
            });

            var dto = new AddOrganizationDto
            {
                Name = "Test Org",
                Description = "Opis",
                ImageLocation = "img/test.jpg",
                OwnerEmail = ownerEmail
            };

            var response = await _httpClient.PostAsJsonAsync("/api/organization", dto);
            var result = await response.Content.ReadFromJsonAsync<Guid>();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotEqual(Guid.Empty, result);

            using var dbContext = GetDbContext();
            var org = await dbContext.Organizations.FindAsync(result);
            Assert.NotNull(org);
            Assert.Equal("Test Org", org.Name);
            Assert.Equal(userId, org.OwnerId);
        }

        [Fact]
        public async Task AddOrganization_ShouldReturnBadRequest_OnServiceError()
        {
            var dto = new AddOrganizationDto
            {
                Name = "Test Org",
                Description = "Opis",
                ImageLocation = "img/test.jpg",
                OwnerEmail = "nonexistent@email.com"
            };

            var response = await _httpClient.PostAsJsonAsync("/api/organization", dto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Error database saving", content);
        }

        [Fact]
        public async Task GetUserOrganizations_ShouldReturnOrganizations()
        {
            var email = "user@email.com";
            var ownerId = Guid.NewGuid();

            await SeedDatabaseAsync(async db =>
            {
                var user = new StandardUser { Id = ownerId, Email = email, Password = "pass", Username = "user" };
                var org = new Organization { Id = Guid.NewGuid(), Name = "TestOrg", OwnerId = user.Id, ImageLocation = "img" };
                db.Users.Add(user);
                db.Organizations.Add(org);
            });

            var response = await _httpClient.GetAsync($"/api/organization/{email}");
            var orgs = await response.Content.ReadFromJsonAsync<List<OrganizationOwnershipDto>>();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(orgs);
            Assert.Single(orgs);
            Assert.Equal("TestOrg", orgs.First().Name);
        }

        [Fact]
        public async Task GetUserOrganizations_ShouldReturnNotFound_IfNoOrganizations()
        {
            var email = "noorguser@email.com";

            var response = await _httpClient.GetAsync($"/api/organization/{email}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("User is not a member or owner of any organization", content);
        }

        [Fact]
        public async Task AddMemberToOrganization_ShouldReturnOk()
        {
            var orgId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();

            await SeedDatabaseAsync(async db =>
            {
                var owner = new StandardUser
                {
                    Id = ownerId,
                    Email = "owner@email.com",
                    Username = "owner",
                    Password = "pass"
                };

                var user = new StandardUser
                {
                    Id = userId,
                    Email = "member@email.com",
                    Username = "member",
                    Password = "pass"
                };

                var org = new Organization
                {
                    Id = orgId,
                    Name = "Org",
                    OwnerId = ownerId,
                    ImageLocation = "img"
                };

                db.Users.Add(owner);
                db.Users.Add(user);
                db.Organizations.Add(org);
            });

            var body = new AddMemberDto { OrganizationId = orgId, UserId = userId };

            var response = await _httpClient.PostAsJsonAsync("/api/organization/add-member", body);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            using var dbContext = GetDbContext();
            var orgWithMembers = await dbContext.Organizations
                .Include(o => o.Members)
                .FirstAsync(o => o.Id == orgId);

            Assert.Contains(orgWithMembers.Members, m => m.Id == userId);
        }

        [Fact]
        public async Task AddMemberToOrganization_ShouldReturnNotFound_IfOrganizationNotFound()
        {
            var body = new AddMemberDto { OrganizationId = Guid.NewGuid(), UserId = Guid.NewGuid() };

            var response = await _httpClient.PostAsJsonAsync("/api/organization/add-member", body);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Organization not found", content);
        }

        [Fact]
        public async Task AddMemberToOrganization_ShouldReturnNotFound_IfUserNotFound()
        {
            var orgId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();

            await SeedDatabaseAsync(async db =>
            {
                var owner = new StandardUser
                {
                    Id = ownerId,
                    Email = "owner@email.com",
                    Username = "owner",
                    Password = "pass"
                };

                var org = new Organization
                {
                    Id = orgId,
                    Name = "Org",
                    OwnerId = ownerId,
                    ImageLocation = "img"
                };

                db.Users.Add(owner);
                db.Organizations.Add(org);
            });

            var body = new AddMemberDto { OrganizationId = orgId, UserId = Guid.NewGuid() }; // UserId koji ne postoji

            var response = await _httpClient.PostAsJsonAsync("/api/organization/add-member", body);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("User not found", content);
        }

        [Fact]
        public async Task DeleteOrganization_ShouldMarkAsDeleted()
        {
            var orgId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();

            await SeedDatabaseAsync(async db =>
            {
                db.Users.Add(new StandardUser
                {
                    Id = ownerId,
                    Email = "owner@email.com",
                    Username = "owner",
                    Password = "pass"
                });

                db.Organizations.Add(new Organization
                {
                    Id = orgId,
                    Name = "ToDelete",
                    OwnerId = ownerId,
                    ImageLocation = "img",
                    IsDeleted = false
                });
            });

            var response = await _httpClient.DeleteAsync($"/api/organization/delete/{orgId}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            using var dbContext = GetDbContext();

            var deletedOrg = await dbContext.Organizations
                .FirstOrDefaultAsync(o => o.Id == orgId);

            Assert.NotNull(deletedOrg);
            Assert.True(deletedOrg.IsDeleted);
        }

        [Fact]
        public async Task DeleteOrganization_ShouldReturnNotFound_IfOrganizationNotFound()
        {
            var nonExistentId = Guid.NewGuid();

            var response = await _httpClient.DeleteAsync($"/api/organization/delete/{nonExistentId}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("not found", content, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task UpdateOrganization_ShouldUpdateSuccessfully()
        {
            var orgId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();

            await SeedDatabaseAsync(async db =>
            {
                var owner = new StandardUser
                {
                    Id = ownerId,
                    Email = "owner@email.com",
                    Username = "owner",
                    Password = "pass"
                };

                var org = new Organization
                {
                    Id = orgId,
                    Name = "Org",
                    OwnerId = ownerId,
                    ImageLocation = "old.jpg"
                };

                db.Users.Add(owner);
                db.Organizations.Add(org);
            });

            var body = new UpdateOrganizationDto
            {
                Id = orgId,
                Description = "New desc",
                ImageLocation = "new.jpg"
            };

            var response = await _httpClient.PutAsJsonAsync("/api/organization/update", body);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            using var dbContext = GetDbContext();
            var orgAfter = await dbContext.Organizations.FindAsync(orgId);
            Assert.Equal("new.jpg", orgAfter.ImageLocation);
            Assert.Equal("New desc", orgAfter.Description);
        }

        [Fact]
        public async Task UpdateOrganization_ShouldReturnBadRequest_IfDtoIsNull()
        {
            var response = await _httpClient.PutAsJsonAsync("/api/organization/update", (object)null);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

    }
}
