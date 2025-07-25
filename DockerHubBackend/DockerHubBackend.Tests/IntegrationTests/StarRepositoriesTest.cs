using DockerHubBackend.Dto.Request;
using DockerHubBackend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DockerHubBackend.Tests.IntegrationTests
{
    public class StarRepositoriesTest : IntegrationTestBase
    {
        [Fact]
        public async Task AddStarRepository_ValidParams_ShouldAddStarRepository()
        {
            using var dbContext = GetDbContext();

            var user = new StandardUser { Email = "user@email.com", Password = "pass", Username = "user", Id = Guid.NewGuid(), Badge = Badge.VerifiedPublisher };
            var repo1 = new DockerRepository { Id = Guid.NewGuid(), Name = "repo1", IsPublic = true, UserOwner = user, UserOwnerId = user.Id, Badge = Badge.VerifiedPublisher, StarCount = 0 };

            dbContext.Users.Add(user);
            dbContext.DockerRepositories.Add(repo1);

            dbContext.SaveChanges();
            dbContext.ChangeTracker.Clear();

            var response = await _httpClient.PatchAsJsonAsync($"/api/dockerRepositories/star/{user.Id.ToString()}/{repo1.Id.ToString()}", new { });
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var changedUser = dbContext.Users
                .OfType<StandardUser>()
                .Include(u => u.StarredRepositories)
                .First(u => u.Id == user.Id);

            Assert.Equal(repo1.Id, changedUser.StarredRepositories.ToList()[0].Id);
            Assert.Equal(1, changedUser.StarredRepositories.ToList()[0].StarCount);
        }

        [Fact]
        public async Task RemoveStarRepository_ValidParams_ShouldRemoveStarRepository()
        {
            using var dbContext = GetDbContext();

            var user = new StandardUser { Email = "user@email.com", Password = "pass", Username = "user", Id = Guid.NewGuid(), Badge = Badge.VerifiedPublisher };
            var repo1 = new DockerRepository { Id = Guid.NewGuid(), Name = "repo1", IsPublic = true, UserOwner = user, UserOwnerId = user.Id, Badge = Badge.VerifiedPublisher, StarCount = 1 };
            user.StarredRepositories.Add(repo1);

            dbContext.Users.Add(user);
            dbContext.DockerRepositories.Add(repo1);

            dbContext.SaveChanges();
            dbContext.ChangeTracker.Clear();

            var response = await _httpClient.PatchAsJsonAsync($"/api/dockerRepositories/star/remove/{user.Id.ToString()}/{repo1.Id.ToString()}", new { });
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var changedUser = dbContext.Users
                .OfType<StandardUser>()
                .Include(u => u.StarredRepositories)
                .First(u => u.Id == user.Id);
            var removedRepository = dbContext.DockerRepositories.Find(repo1.Id);

            Assert.Empty(changedUser.StarredRepositories);
            Assert.Equal(0, removedRepository.StarCount);
        }

        [Fact]
        public async Task StarRepository_InvalidParams_ShouldThrowException()
        {
            var response = await _httpClient.PatchAsJsonAsync($"/api/dockerRepositories/star/bad_user_id/bad_repo_id", new { });
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            response = await _httpClient.PatchAsJsonAsync($"/api/dockerRepositories/star/remove/bad_user_id/bad_repo_id", new { });
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
