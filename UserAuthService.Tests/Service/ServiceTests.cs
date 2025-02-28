using Microsoft.EntityFrameworkCore;
using UserAuthService.Data;
using UserAuthService.DTOs;
using UserAuthService.Models.Auth;
using UserAuthService.Services;

namespace UserAuthService.Tests.ServiceTests
{
    public class ServiceTests
    {
        public class ApiKeyServiceTests
        {
            [Fact]
            public async Task GenerateApiKeyAsync_ValidClientId_ReturnsApiKey()
            {
                // Arrange
                string clientId = "testClient";
                var options = new DbContextOptionsBuilder<UserDbContext>()
                    .UseInMemoryDatabase("TestDb")
                    .Options;

                using (var dbContext = new UserDbContext(options))
                {
                    var apiKeyService = new ApiKeyService(dbContext);

                    // Act
                    var result = await apiKeyService.GenerateApiKeyAsync(clientId);

                    // Assert
                    Assert.NotNull(result);
                    Assert.Equal(clientId, result.ClientId);
                    Assert.Equal(64, result.Key.Length);
                }
            }

            [Fact]
            public async Task ValidateApiKeyAsync_ValidApiKey_ReturnsTrue()
            {
                // Arrange
                var options = new DbContextOptionsBuilder<UserDbContext>()
                    .UseInMemoryDatabase("TestDb")
                    .Options;

                using (var dbContext = new UserDbContext(options))
                {
                    // Seed the in-memory database with a valid API key
                    var apiKeyEntity = new ApiKey
                    {
                        Key = "validApiKey",
                        ValidUntil = DateTime.UtcNow.AddDays(1),
                        ClientId = "test",
                        Id = Guid.NewGuid()
                    };

                    dbContext.ApiKeys.Add(apiKeyEntity);
                    await dbContext.SaveChangesAsync();

                    // Create an instance of the ApiKeyService
                    var apiKeyService = new ApiKeyService(dbContext);

                    // Act
                    var isValid = await apiKeyService.ValidateApiKeyAsync("validApiKey");

                    // Assert
                    Assert.True(isValid);
                }
            }

            [Fact]
            public async Task ValidateApiKeyAsync_InvalidApiKey_ReturnsFalse()
            {
                // Arrange
                var options = new DbContextOptionsBuilder<UserDbContext>()
                    .UseInMemoryDatabase("TestDb") 
                    .Options;

                using (var dbContext = new UserDbContext(options))
                {
                    // Seed the in-memory database with no API keys
                    dbContext.ApiKeys.AddRange();
                    await dbContext.SaveChangesAsync();

                    // Create an instance of the ApiKeyService
                    var apiKeyService = new ApiKeyService(dbContext);

                    // Act
                    var isValid = await apiKeyService.ValidateApiKeyAsync("invalidApiKey");

                    // Assert
                    Assert.False(isValid);
                }
            }

            [Fact]
            public async Task ValidateApiKeyAsync_ExpiredApiKey_ReturnsFalse()
            {
                // Arrange
                var options = new DbContextOptionsBuilder<UserDbContext>()
                    .UseInMemoryDatabase("TestDb")
                    .Options;

                using (var dbContext = new UserDbContext(options))
                {
                    var apiKeyEntity = new ApiKey
                    {
                        Key = "expiredApiKey",
                        ValidUntil = DateTime.UtcNow.AddDays(-1),
                        ClientId = "test",
                        Id = Guid.NewGuid()
                    };

                    dbContext.ApiKeys.Add(apiKeyEntity);
                    await dbContext.SaveChangesAsync();

                    // Create an instance of the ApiKeyService
                    var apiKeyService = new ApiKeyService(dbContext);

                    // Act
                    var isValid = await apiKeyService.ValidateApiKeyAsync("expiredApiKey");

                    // Assert
                    Assert.False(isValid);
                }
            }

        }

        public class UserServiceTests
        {
            [Fact]
            public async Task GetAllUsersAsync_ReturnsAllUsers()
            {
                // Arrange
                var options = new DbContextOptionsBuilder<UserDbContext>()
                    .UseInMemoryDatabase("TestDb")
                    .Options;

                using (var dbContext = new UserDbContext(options))
                {
                    // Ensure the database is clean before the test
                    dbContext.Database.EnsureDeleted();
                    dbContext.Database.EnsureCreated();

                    // Add sample users to the in-memory database
                    var users = new List<User>
                    {
                        new User { Id = Guid.NewGuid(), UserName = "user1", FullName = "User One",
                            PasswordHash = "hashed_password", Email = "test@test.test",
                            Culture = "culture", MobileNumber = "123456789", Language = "en" },

                        new User { Id = Guid.NewGuid(), UserName = "user2", FullName = "User Two",
                            PasswordHash = "hashed_password", Email = "test@test.test",
                            Culture = "culture", MobileNumber = "123456789", Language = "en" }
                    };

                    dbContext.Users.AddRange(users);
                    await dbContext.SaveChangesAsync();

                    // Create an instance of the UserService with the in-memory dbContext
                    var userService = new UserService(dbContext);

                    // Act
                    var result = await userService.GetAllUsersAsync();

                    // Assert
                    Assert.NotNull(result);
                    Assert.Equal(2, result.Count());
                }
            }


            [Fact]
            public async Task GetUserByIdAsync_ExistingUser_ReturnsUser()
            {
                // Arrange
                var options = new DbContextOptionsBuilder<UserDbContext>()
                    .UseInMemoryDatabase("TestDb")
                    .Options;

                using (var dbContext = new UserDbContext(options))
                {
                    var userId = Guid.NewGuid();
                    var user = new User
                    {
                        Id = userId,
                        UserName = "testuser",
                        FullName = "Test User",
                        PasswordHash = "hashed_password",
                        Email = "test@test.test",
                        Culture = "culture",
                        MobileNumber = "123456789",
                        Language = "en"
                    };

                    dbContext.Users.Add(user);
                    await dbContext.SaveChangesAsync(); 

                    // Create an instance of the UserService with the in-memory dbContext
                    var userService = new UserService(dbContext);

                    // Act
                    var result = await userService.GetUserByIdAsync(userId);

                    // Assert
                    Assert.NotNull(result); // Ensure the result is not null
                    Assert.Equal(userId, result.Id); // Ensure the user ID matches
                    Assert.Equal("testuser", result.UserName); // Ensure the username matches
                }
            }


            [Fact]
            public async Task GetUserByIdAsync_NonExistingUser_ReturnsNull()
            {
                // Arrange
                var options = new DbContextOptionsBuilder<UserDbContext>()
                    .UseInMemoryDatabase("TestDb")
                    .Options;

                using (var dbContext = new UserDbContext(options))
                {
                    // Create an instance of the UserService with the in-memory dbContext
                    var userService = new UserService(dbContext);

                    // Act
                    var result = await userService.GetUserByIdAsync(Guid.NewGuid()); // Using a random GUID

                    // Assert
                    Assert.Null(result); // Assert that the result is null because the user does not exist
                }
            }

            [Fact]
            public async Task UpdateUserAsync_ExistingUser_ReturnsUpdatedUser()
            {
                // Arrange
                var options = new DbContextOptionsBuilder<UserDbContext>()
                    .UseInMemoryDatabase("TestDb")
                    .Options;

                using (var dbContext = new UserDbContext(options))
                {
                    var userId = Guid.NewGuid();
                    var updateUserDto = new UpdateUserDto { FullName = "Updated User" };

                    var existingUser = new User
                    {
                        Id = userId,
                        UserName = "testuser",
                        FullName = "Original User",
                        PasswordHash = "hashed_password",
                        Email = "test@test.test",
                        Culture = "culture",
                        MobileNumber = "123456789",
                        Language = "en"
                    };
                    dbContext.Users.Add(existingUser);
                    await dbContext.SaveChangesAsync();

                    // Create an instance of the UserService with the in-memory dbContext
                    var userService = new UserService(dbContext);

                    // Act
                    var updatedUser = await userService.UpdateUserAsync(userId, updateUserDto);

                    // Assert
                    Assert.NotNull(updatedUser);
                    Assert.Equal(userId, updatedUser.Id);
                    Assert.Equal("Updated User", updatedUser.FullName);

                    // Verify that the user was updated in the in-memory database
                    var userInDb = await dbContext.Users.FindAsync(userId);
                    Assert.NotNull(userInDb);
                    Assert.Equal("Updated User", userInDb.FullName);
                }
            }

            [Fact]
            public async Task DeleteUserAsync_ExistingUser_DeletesUser()
            {
                // Arrange
                var options = new DbContextOptionsBuilder<UserDbContext>()
                    .UseInMemoryDatabase("TestDb")
                    .Options;

                using (var dbContext = new UserDbContext(options))
                {
                    dbContext.Database.EnsureDeleted();
                    dbContext.Database.EnsureCreated();

                    var userId = Guid.NewGuid();
                    var existingUser = new User
                    {
                        Id = userId,
                        UserName = "testuser",
                        FullName = "Test User",
                        PasswordHash = "hashed_password",
                        Email = "test@test.test",
                        Culture = "culture",
                        MobileNumber = "123456789",
                        Language = "en"
                    };

                    dbContext.Users.Add(existingUser);
                    await dbContext.SaveChangesAsync();

                    // Create an instance of the UserService with the in-memory dbContext
                    var userService = new UserService(dbContext);

                    // Act
                    await userService.DeleteUserAsync(userId);

                    // Assert
                    var deletedUser = await dbContext.Users.FindAsync(userId);
                    Assert.Null(deletedUser);

                    Assert.Empty(dbContext.Users);
                }
            }


            [Fact]
            public async Task ValidatePasswordAsync_ValidCredentials_ReturnsTrue()
            {
                // Arrange
                var options = new DbContextOptionsBuilder<UserDbContext>()
                    .UseInMemoryDatabase("TestDb") // Use in-memory database for the test
                    .Options;

                using (var dbContext = new UserDbContext(options))
                {
                    dbContext.Database.EnsureDeleted();
                    dbContext.Database.EnsureCreated();

                    string userName = "testuser";
                    string password = "password";
                    var user = new User
                    {
                        Id = Guid.NewGuid(),
                        UserName = userName,
                        PasswordHash = Utilities.PasswordHasher.HashPassword(password),
                        Email = "test@test.test",
                        Culture = "culture",
                        MobileNumber = "123456789",
                        Language = "en",
                        FullName = "Test User"
                    };

                    dbContext.Users.Add(user);
                    await dbContext.SaveChangesAsync();

                    // Create an instance of the UserService with the in-memory dbContext
                    var userService = new UserService(dbContext);

                    // Act
                    var isValid = await userService.ValidatePasswordAsync(userName, password);

                    // Assert
                    Assert.True(isValid);
                }
            }

            [Fact]
            public async Task ValidatePasswordAsync_InvalidCredentials_ReturnsFalse()
            {
                // Arrange
                var options = new DbContextOptionsBuilder<UserDbContext>()
                    .UseInMemoryDatabase("TestDb") 
                    .Options;

                using (var dbContext = new UserDbContext(options))
                {
                    string userName = "testuser";
                    string correctPassword = "password"; 
                    string invalidPassword = "wrongpassword";

                    var user = new User
                    {
                        Id = Guid.NewGuid(),
                        UserName = userName,
                        PasswordHash = Utilities.PasswordHasher.HashPassword(correctPassword),
                        Email = "test@test.test",
                        Culture = "culture",
                        MobileNumber = "123456789",
                        Language = "en",
                        FullName = "Test User"
                    };

                    dbContext.Users.Add(user);
                    await dbContext.SaveChangesAsync();

                    var userService = new UserService(dbContext);

                    // Act
                    var isValid = await userService.ValidatePasswordAsync(userName, invalidPassword);

                    // Assert
                    Assert.False(isValid);
                }
            }

        }
    }
}