using DockerHubBackend.Dto.Request;
using DockerHubBackend.Dto.Response;
using DockerHubBackend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DockerHubBackend.Tests.IntegrationTests
{
    public class ChangeBadgeTest : IntegrationTestBase
    {
        [Fact]
        public async Task ChangeBadge_ValidParams_ShouldChangeBadge()
        {
            using var dbContext = GetDbContext();

            var user = new StandardUser { Email = "user@email.com", Password = "pass", Username = "user", Id = Guid.NewGuid(), Badge = Badge.VerifiedPublisher };
            var repo1 = new DockerRepository { Id = Guid.NewGuid(), Name = "repo1", IsPublic = true, UserOwner = user, UserOwnerId = user.Id, Badge = Badge.VerifiedPublisher };
            var img1 = new DockerImage { Id = Guid.NewGuid(), DockerRepositoryId = repo1.Id, Repository = repo1, Digest = "123" };

            dbContext.Users.Add(user);
            dbContext.DockerRepositories.Add(repo1);
            dbContext.DockerImages.Add(img1);

            dbContext.SaveChanges();
            dbContext.ChangeTracker.Clear();

            var response = await _httpClient.PatchAsJsonAsync($"/api/user/{user.Id.ToString()}/badge/change", new NewBadgeDTO { Badge = "SponsoredOSS" });
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var changedUser = dbContext.Users.First(u => u.Id == user.Id);
            var changedRepo = dbContext.DockerRepositories.First(r => r.Id == repo1.Id);

            Assert.Equal(Badge.SponsoredOSS, changedUser.Badge);
            Assert.Equal(Badge.SponsoredOSS, changedRepo.Badge);
        }

        [Fact]
        public async Task ChangeBadge_WithBadBadgeValue_ShouldThrowException()
        {
            using var dbContext = GetDbContext();

            var user = new StandardUser { Email = "user@email.com", Password = "pass", Username = "user", Id = Guid.NewGuid(), Badge = Badge.VerifiedPublisher };
            var repo1 = new DockerRepository { Id = Guid.NewGuid(), Name = "repo1", IsPublic = true, UserOwner = user, UserOwnerId = user.Id, Badge = Badge.VerifiedPublisher };
            var img1 = new DockerImage { Id = Guid.NewGuid(), DockerRepositoryId = repo1.Id, Repository = repo1, Digest = "123" };

            dbContext.Users.Add(user);
            dbContext.DockerRepositories.Add(repo1);
            dbContext.DockerImages.Add(img1);

            dbContext.SaveChanges();
            dbContext.ChangeTracker.Clear();

            var response = await _httpClient.PatchAsJsonAsync($"/api/user/{user.Id.ToString()}/badge/change", new NewBadgeDTO { Badge = "Bad badge value" });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ChangeBadge_WithInvalidUserId_ShouldThrowException()
        {
            using var dbContext = GetDbContext();

            var user = new StandardUser { Email = "user@email.com", Password = "pass", Username = "user", Id = Guid.NewGuid(), Badge = Badge.VerifiedPublisher };
            var repo1 = new DockerRepository { Id = Guid.NewGuid(), Name = "repo1", IsPublic = true, UserOwner = user, UserOwnerId = user.Id, Badge = Badge.VerifiedPublisher };
            var img1 = new DockerImage { Id = Guid.NewGuid(), DockerRepositoryId = repo1.Id, Repository = repo1, Digest = "123" };

            dbContext.Users.Add(user);
            dbContext.DockerRepositories.Add(repo1);
            dbContext.DockerImages.Add(img1);

            dbContext.SaveChanges();
            dbContext.ChangeTracker.Clear();

            var response = await _httpClient.PatchAsJsonAsync($"/api/user/bad_id/badge/change", new NewBadgeDTO { Badge = "VerifiedPublisher" });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
