using DockerHubBackend.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace DockerHubBackend.Data
{

    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<BaseUser> Users { get; set; }
        public DbSet<DockerImage> DockerImages { get; set; }
        public DbSet<DockerRepository> DockerRepositories { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<TeamPermission> TeamPermissions { get; set; }
        public DbSet<VerificationToken> VerificationTokens { get; set; }
        public DbSet<ImageTag> ImageTags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Organization>().ToTable("Organizations");

            modelBuilder.Entity<Organization>()
                .HasMany(o => o.Members)
                .WithMany(m => m.MemberOrganizations);
            modelBuilder.Entity<StandardUser>()
                .HasMany(u => u.MyOrganizations)
                .WithOne(o => o.Owner);
            modelBuilder.Entity<BaseUser>()
                .HasMany(u => u.MyRepositories)
                .WithOne(r => r.UserOwner);
            modelBuilder.Entity<Organization>()
                .HasMany(o => o.Repositories)
                .WithOne(r => r.OrganizationOwner);

            modelBuilder.Entity<BaseUser>()
                .HasDiscriminator<string>("Discriminator")
                .HasValue<Admin>("Admin")
                .HasValue<SuperAdmin>("SuperAdmin")
                .HasValue<StandardUser>("StandardUser");

  
            modelBuilder.Entity<BaseUser>().ToTable("Users");

            modelBuilder.Entity<StandardUser>()
                .HasMany(s => s.MemberOrganizations)
                .WithMany(o => o.Members)
                .UsingEntity<Dictionary<string, object>>(
                    "OrganizationMembers",
                    join => join
                        .HasOne<Organization>()
                        .WithMany()
                        .HasForeignKey("OrganizationId")
                        .HasConstraintName("FK_Membership_Organization"),
                    join => join
                        .HasOne<StandardUser>()
                        .WithMany()
                        .HasForeignKey("MemberId")
                        .HasConstraintName("FK_Membership_User"));

            modelBuilder.Entity<StandardUser>()
            .HasMany(s => s.StarredRepositories)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "StarredRepositories",
                join => join
                    .HasOne<DockerRepository>()
                    .WithMany()
                    .HasForeignKey("RepositoryId")
                    .HasConstraintName("FK_Star_Repository"),
                join => join
                    .HasOne<StandardUser>()
                    .WithMany()
                    .HasForeignKey("UserId")
                    .HasConstraintName("FK_Star_User"));

            modelBuilder.Entity<BaseUser>()
                .HasIndex(u => u.Username)
                .IsUnique();
            modelBuilder.Entity<BaseUser>()
                .HasIndex(u => u.Email)
                .IsUnique();
            modelBuilder.Entity<VerificationToken>()
                .HasIndex(vt => vt.UserId)
                .IsUnique();
            modelBuilder.Entity<VerificationToken>()
                .HasIndex(vt => vt.Token)
                .IsUnique();
            modelBuilder.Entity<VerificationToken>()
                .HasOne<BaseUser>(vt => vt.User)
                .WithOne(u => u.VerificationToken)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<DockerImage>()
                .HasIndex(di => di.Digest)
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}
