using Microsoft.EntityFrameworkCore;
using UserAuthService.Utilities;
using UserAuthService.Models.Auth;

namespace UserAuthService.Data
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<ApiKey> ApiKeys { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<ApiKey>().ToTable("ApiKeys");
        }

        public static void SeedData(UserDbContext context)
        {
            if (!context.Users.Any())
            {
                context.Users.AddRange(
                    new User
                    {
                        Id = Guid.NewGuid(),
                        UserName = "admin",
                        FullName = "Admin User",
                        Email = "admin@example.com",
                        MobileNumber = "1234567890",
                        Language = "en",
                        Culture = "en-US",
                        PasswordHash = PasswordHasher.HashPassword("password1")
                    },
                    new User
                    {
                        Id = Guid.NewGuid(),
                        UserName = "janez_n",
                        FullName = "Janez Novak",
                        Email = "n.janez@example.com",
                        MobileNumber = "9876543210",
                        Language = "en",
                        Culture = "en-US",
                        PasswordHash = PasswordHasher.HashPassword("password2")
                    }
                );
                context.SaveChanges();
            }

            if (!context.ApiKeys.Any())
            {
                context.ApiKeys.AddRange(
                    new ApiKey
                    {
                        Id = Guid.NewGuid(),
                        Key = ApiKeyGenerator.GenerateSecureApiKey(),
                        ClientId = "Redacted Client #1",
                        ValidUntil = DateTime.UtcNow.AddYears(1)
                    },
                    new ApiKey
                    {
                        Id = Guid.NewGuid(),
                        Key = "keySampleForTesting",
                        ClientId = "Redacted Client #2",
                        ValidUntil = DateTime.UtcNow.AddYears(1)
                    }
                );
                context.SaveChanges();
            }
        }
    }
}