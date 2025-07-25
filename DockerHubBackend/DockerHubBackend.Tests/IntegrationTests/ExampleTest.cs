using DockerHubBackend.Dto.Request;
using DockerHubBackend.Dto.Response;
using DockerHubBackend.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace DockerHubBackend.Tests.IntegrationTests
{
    public class ExampleTest : IntegrationTestBase
    {
        [Fact]
        public async Task Example_Test()
        {
            using var dbContext = GetDbContext();

            var user = new StandardUser { Email = "user@email.com", Password = "pass", Username = "user", Id = Guid.NewGuid() };
            var repo = new DockerRepository { Id = Guid.NewGuid(), Name = "repo", IsPublic = true, UserOwner = user, UserOwnerId = user.Id };
            var img = new DockerImage { Id = Guid.NewGuid(), DockerRepositoryId = repo.Id, Repository = repo, Digest = "123" };

            dbContext.Users.Add(user);
            dbContext.DockerRepositories.Add(repo);
            dbContext.DockerImages.Add(img);

            dbContext.SaveChanges();

            var response = await _httpClient.GetAsync("/api/dockerImages?page=1&pageSize=10");
            response.EnsureSuccessStatusCode();

            // Ovaj test pokreni sa komandom dotnet test -v n --filter "FullyQualifiedName~Example_Test"
            // tako ce se pokrenuti samo ovaj test u verbose modu i onda ce se videti Console.WriteLine
            // samo sa dotnet test nece da se vide ispisi
            var responseString = await response.Content.ReadAsStringAsync();
            Console.WriteLine("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            Console.WriteLine(responseString);
            Console.WriteLine("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            var pageDto = JsonSerializer.Deserialize<PageDTO<DockerImageDTO>>(responseString, _jsonSerializerOptions);

            Assert.Equal(1, pageDto.TotalNumberOfElements);
            Assert.Equal(img.Id.ToString(), pageDto.Data[0].ImageId);

            response = await _httpClient.GetAsync("/api/dockerImages?page=2&pageSize=1");
            responseString = await response.Content.ReadAsStringAsync();
            pageDto = JsonSerializer.Deserialize<PageDTO<DockerImageDTO>>(responseString, _jsonSerializerOptions);
            Assert.Equal(1, pageDto.TotalNumberOfElements);
            Assert.Equal(0, pageDto.Data.Count);
        }
    }
}
