using DockerHubBackend.Models;
using Microsoft.AspNetCore.Identity;

namespace DockerHubBackend.Data
{
    public static class DatabaseContextSeed
    {
        public static async Task SeedDataAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DataContext>();
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<string>>();

            var passwordHash = passwordHasher.HashPassword(String.Empty, "123456");
            if (!context.Users.Any())
            {
                context.Users.AddRange(
                    new StandardUser { Username = "User1", Email = "user1@email.com", Password = passwordHash },
                    new StandardUser { Username = "User2", Email = "user2@email.com", Password = passwordHash },
                    new SuperAdmin { Username = "SuperAdmin", Email = "super.admin@email.com", Password = passwordHash, IsVerified = false}
                );
                await context.SaveChangesAsync();
            }

            if (!context.Organizations.Any())
            {
                context.Organizations.AddRange(
                    new Organization 
                    {  
                        Name = "Code org",
                        Description = "This is some organization. This is an example of org description.", 
                        ImageLocation = "some_loc", 
                        OwnerId = context.Users.OrderBy(u => u.Id).First().Id,
                        Owner = (StandardUser) context.Users.OrderBy(u => u.Id).First(),
                    }    
                );
                await context.SaveChangesAsync();
            }

            /*if (!context.Teams.Any()) 
            {
                HashSet<StandardUser> members = new HashSet<StandardUser>();
                members.Add((StandardUser)context.Users.OrderBy(u => u.Id).Last());
                context.Teams.AddRange(
                    new Team 
                    {  
                        Name = "Team 1", Description="Some desc",
                        Members = members,
                        Organization = context.Organizations.OrderBy(o => o.Id).First(),
                        OrganizationId = context.Organizations.OrderBy(o => o.Id).First().Id
                    },
                     new Team
                     {
                         Name = "Team 2",
                         Description = "Some desc",
                         Members = members,
                         Organization = context.Organizations.OrderBy(o => o.Id).First(),
                         OrganizationId = context.Organizations.OrderBy(o => o.Id).First().Id
                     }
                );

                await context.SaveChangesAsync();
            }*/

            if (!context.DockerRepositories.Any())
            {
                context.DockerRepositories.AddRange(
                    new DockerRepository
                    {
                        Name = "My First Repository",
                        Description = "This is an example of some description.",
                        Teams = context.Teams.OrderBy(o => o.Id).ToList(),
                    }
                );

                await context.SaveChangesAsync();
            }
        }
    }
}
