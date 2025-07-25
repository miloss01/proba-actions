using DockerHubBackend.Data;
using DockerHubBackend.Startup;
using DockerHubBackend.Tests.IntegrationTests;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

public class IntegrationTestBase : IAsyncLifetime
{
    protected readonly HttpClient _httpClient;
    protected readonly IServiceScopeFactory _scopeFactory;
    protected readonly JsonSerializerOptions _jsonSerializerOptions; 

    public IntegrationTestBase()
    {
        // Kreiramo WebApplicationFactory za testiranje API-ja
        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IHostedService) &&
                                                              d.ImplementationType == typeof(StartupScript));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
                    services.AddDbContext<DataContext>(options =>
                        options.UseNpgsql("Host=localhost;Port=5432;Username=admin;Password=admin;Database=test-uks-database"));
                });
            });

        _httpClient = factory.CreateClient();
        _scopeFactory = factory.Services.GetRequiredService<IServiceScopeFactory>();
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    // Resetovanje baze pre svakog testa
    public async Task InitializeAsync()
    {
        await ResetDatabase();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    // Metoda za resetovanje baze
    private async Task ResetDatabase()
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();

        await dbContext.Database.EnsureDeletedAsync(); // Brisanje baze
        await dbContext.Database.EnsureCreatedAsync(); // Ponovno kreiranje baze
    }

    // Metoda za ubacivanje podataka u bazu
    protected async Task SeedDatabaseAsync(Func<DataContext, Task> seeder)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();

        await seeder(dbContext);
        await dbContext.SaveChangesAsync();
    }

    // Metoda za direktan pristup bazi iz testa
    protected DataContext GetDbContext()
    {
        var scope = _scopeFactory.CreateScope();
        return scope.ServiceProvider.GetRequiredService<DataContext>();
    }
}
