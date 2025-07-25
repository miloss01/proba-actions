using DockerHubBackend.Config;
using DockerHubBackend.Data;
using DockerHubBackend.Filters;
using DockerHubBackend.Repository.Implementation;
using DockerHubBackend.Repository.Interface;
using DockerHubBackend.Security;
using DockerHubBackend.Services.Implementation;
using DockerHubBackend.Services.Interface;
using DockerHubBackend.Startup;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using Nest;
using Serilog.Formatting.Elasticsearch;
using DockerHubBackend.Repository.Utils;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

builder.Services.Configure<AwsSettings>(builder.Configuration.GetSection("AWS"));

var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtCookieName = builder.Configuration["JWT:CookieName"];

String postgreConnectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") ?? 
    "Host=localhost;Port=5432;Database=uks-database;Username=admin;Password=admin";
builder.Configuration["ConnectionStrings:DefaultConnection"] = postgreConnectionString;

// Add services to the container.

// Authentication
builder.Services.AddScoped<IPasswordHasher<string>, BCryptPasswordHasher>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtIssuer,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!))
    };
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var claimsPrincipal = context.Principal;
            Guid userId;
            if(!Guid.TryParse(claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier), out userId))
            {
                context.Fail("Token is invalid");
            }
            var dbContext = context.HttpContext.RequestServices.GetRequiredService<DataContext>();
            var user = await dbContext.Users.FindAsync(userId);

            if (user == null || user.LastPasswordChangeDate > context.SecurityToken.ValidFrom)
            {
                context.Fail("Token is invalid due to password change");
            }
        }
    };
});
builder.Services.AddSingleton<IJwtHelper,JwtHelper>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "CORS_CONFIG",
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:4200").AllowAnyMethod().AllowAnyHeader().AllowCredentials();
                      });
});

// Database
builder.Services.AddDbContext<DataContext>(opt => opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repository
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IVerificationTokenRepository, VerificationTokenRepository>();
builder.Services.AddScoped<IDockerImageRepository, DockerImageRepository>();
builder.Services.AddScoped<IDockerRepositoryRepository, DockerRepositoryRepository>();
builder.Services.AddScoped<IOrganizationRepository, OrganizationRepository>();
builder.Services.AddScoped<ITeamRepository, TeamRepository>();

// Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<IRandomTokenGenerator, RandomTokenGenerator>();
builder.Services.AddScoped<IDockerImageService, DockerImageService>();
builder.Services.AddScoped<IDockerRepositoryService, DockerRepositoryService>();
builder.Services.AddScoped<IOrganizationService, OrganizationService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<IRegistryService, RegistryService>();
builder.Services.AddScoped<IUnitOfWork, EfUnitOfWork>();
builder.Services.AddScoped<ILogService, LogService>();


builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers(options =>
{
    options.Filters.Add<GlobalExceptionHandler>();
});

builder.Services.AddSingleton<IElasticClient>(new ElasticClient(
    new ConnectionSettings(new Uri("http://localhost:9200"))
        .DefaultIndex("logstash-*")
        .DisableDirectStreaming()
    ));
builder.Services.AddHostedService<LogService>();
builder.Services.AddHostedService<StartupScript>();

// confing serilog 
builder.Host.UseSerilog((context, config) =>
{
    config
        //.MinimumLevel.Information() 
        .WriteTo.Console()
        .WriteTo.File("Logs/log-.log", rollingInterval: RollingInterval.Day);
        //.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
        //{
        //    AutoRegisterTemplate = true,
        //    IndexFormat = "logstash-{0:yyyy.MM.dd}",  // Format logova u Elasticsearch-u
        //    AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7, // Dodajte ovo ako koristite Elasticsearch 7+
        //    EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog,
        //    CustomFormatter = new ElasticsearchJsonFormatter()
        //});
});

var port = Environment.GetEnvironmentVariable("PORT") ?? "5156";

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(int.Parse(port));
});

var app = builder.Build();

//await DatabaseContextSeed.SeedDataAsync(app.Services);

// Configure the HTTP request pipeline.
var applyMigrations = Environment.GetEnvironmentVariable("APPLY_MIGRATIONS") == "true";
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    if (applyMigrations) app.ApplyMigrations();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseCors("CORS_CONFIG");

app.MapControllers();

app.Run();

public partial class Program { }
