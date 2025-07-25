using DockerHubBackend.Dto.Response;
using DockerHubBackend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DockerHubBackend.Tests.IntegrationTests
{
    public class ExploreSectionTest : IntegrationTestBase
    {
        [Fact]
        public async Task GettingDockerImages_PassAllQueryParams_ReturnsValidImages()
        {
            using var dbContext = GetDbContext();

            var user = new StandardUser { Email = "user@email.com", Password = "pass", Username = "user", Id = Guid.NewGuid() };
            var repo1 = new DockerRepository { Id = Guid.NewGuid(), Name = "repo1", IsPublic = true, UserOwner = user, UserOwnerId = user.Id, Badge = Badge.VerifiedPublisher };
            var repo2 = new DockerRepository { Id = Guid.NewGuid(), Name = "repo2", IsPublic = true, UserOwner = user, UserOwnerId = user.Id };
            var img1 = new DockerImage { Id = Guid.NewGuid(), DockerRepositoryId = repo1.Id, Repository = repo1, Digest = "123" };
            var img2 = new DockerImage { Id = Guid.NewGuid(), DockerRepositoryId = repo2.Id, Repository = repo2, Digest = "456" };

            dbContext.Users.Add(user);
            dbContext.DockerRepositories.Add(repo1);
            dbContext.DockerRepositories.Add(repo2);
            dbContext.DockerImages.Add(img1);
            dbContext.DockerImages.Add(img2);

            dbContext.SaveChanges();

            var response = await _httpClient.GetAsync("/api/dockerImages?page=1&pageSize=10");
            var responseString = await response.Content.ReadAsStringAsync();
            var pageDto = JsonSerializer.Deserialize<PageDTO<DockerImageDTO>>(responseString, _jsonSerializerOptions);

            Assert.Equal(2, pageDto.TotalNumberOfElements);
            Assert.Contains(pageDto.Data[0].ImageId, img1.Id.ToString() + img2.Id.ToString());
            Assert.Contains(pageDto.Data[1].ImageId, img1.Id.ToString() + img2.Id.ToString());

            response = await _httpClient.GetAsync("/api/dockerImages?page=3&pageSize=1");
            responseString = await response.Content.ReadAsStringAsync();
            pageDto = JsonSerializer.Deserialize<PageDTO<DockerImageDTO>>(responseString, _jsonSerializerOptions);
            Assert.Equal(2, pageDto.TotalNumberOfElements);
            Assert.Empty(pageDto.Data);

            response = await _httpClient.GetAsync("/api/dockerImages?page=1&pageSize=10&searchTerm=repo1");
            responseString = await response.Content.ReadAsStringAsync();
            pageDto = JsonSerializer.Deserialize<PageDTO<DockerImageDTO>>(responseString, _jsonSerializerOptions);
            Assert.Equal(1, pageDto.TotalNumberOfElements);
            Assert.Equal(img1.Id.ToString(), pageDto.Data[0].ImageId);

            response = await _httpClient.GetAsync("/api/dockerImages?page=1&pageSize=10&badges=VerifiedPublisher");
            responseString = await response.Content.ReadAsStringAsync();
            pageDto = JsonSerializer.Deserialize<PageDTO<DockerImageDTO>>(responseString, _jsonSerializerOptions);
            Assert.Equal(1, pageDto.TotalNumberOfElements);
            Assert.Equal(img1.Id.ToString(), pageDto.Data[0].ImageId);
        }

        [Fact]
        public async Task GettingDockerRepositories_PassAllQueryParams_ReturnsValidRepositories()
        {
            using var dbContext = GetDbContext();

            var user = new StandardUser { Email = "user@email.com", Password = "pass", Username = "user", Id = Guid.NewGuid() };
            var repo1 = new DockerRepository { Id = Guid.NewGuid(), Name = "repo1", IsPublic = true, UserOwner = user, UserOwnerId = user.Id, Badge = Badge.VerifiedPublisher };
            var repo2 = new DockerRepository { Id = Guid.NewGuid(), Name = "repo2", IsPublic = true, UserOwner = user, UserOwnerId = user.Id };

            dbContext.Users.Add(user);
            dbContext.DockerRepositories.Add(repo1);
            dbContext.DockerRepositories.Add(repo2);

            dbContext.SaveChanges();

            var response = await _httpClient.GetAsync("/api/dockerRepositories?page=1&pageSize=10");
            var responseString = await response.Content.ReadAsStringAsync();
            var pageDto = JsonSerializer.Deserialize<PageDTO<DockerRepositoryDTO>>(responseString, _jsonSerializerOptions);

            Assert.Equal(2, pageDto.TotalNumberOfElements);
            Assert.Contains(pageDto.Data[0].Id, repo1.Id.ToString() + repo2.Id.ToString());
            Assert.Contains(pageDto.Data[1].Id, repo1.Id.ToString() + repo2.Id.ToString());

            response = await _httpClient.GetAsync("/api/dockerRepositories?page=3&pageSize=1");
            responseString = await response.Content.ReadAsStringAsync();
            pageDto = JsonSerializer.Deserialize<PageDTO<DockerRepositoryDTO>>(responseString, _jsonSerializerOptions);
            Assert.Equal(2, pageDto.TotalNumberOfElements);
            Assert.Empty(pageDto.Data);

            response = await _httpClient.GetAsync("/api/dockerRepositories?page=1&pageSize=10&searchTerm=repo1");
            responseString = await response.Content.ReadAsStringAsync();
            pageDto = JsonSerializer.Deserialize<PageDTO<DockerRepositoryDTO>>(responseString, _jsonSerializerOptions);
            Assert.Equal(1, pageDto.TotalNumberOfElements);
            Assert.Equal(repo1.Id.ToString(), pageDto.Data[0].Id);

            response = await _httpClient.GetAsync("/api/dockerRepositories?page=1&pageSize=10&badges=VerifiedPublisher");
            responseString = await response.Content.ReadAsStringAsync();
            pageDto = JsonSerializer.Deserialize<PageDTO<DockerRepositoryDTO>>(responseString, _jsonSerializerOptions);
            Assert.Equal(1, pageDto.TotalNumberOfElements);
            Assert.Equal(repo1.Id.ToString(), pageDto.Data[0].Id);
        }
    }
}
