using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UserAuthService.DTOs;
using UserAuthService.Models.Auth;
using UserAuthService.Services;

namespace UserAuthService.Tests.ControllerTests
{
    public class ControllerTests
    {
        public class ApiKeysControllerTests
        {
            private readonly Mock<IApiKeyService> _mockApiKeyService;
            private readonly ApiKeysController _controller;

            public ApiKeysControllerTests()
            {
                _mockApiKeyService = new Mock<IApiKeyService>();
                _controller = new ApiKeysController(_mockApiKeyService.Object);
            }

            [Fact]
            public async Task GenerateApiKey_ValidClientId_ReturnsOkResult()
            {
                // Arrange
                string clientId = "testClient";
                var apiKey = new ApiKey { Key = "testapikey", ClientId = clientId,
                    ValidUntil = DateTime.UtcNow.AddDays(1), Id = Guid.NewGuid() };
                _mockApiKeyService.Setup(x => x.GenerateApiKeyAsync(clientId)).ReturnsAsync(apiKey);

                // Act
                var result = await _controller.GenerateApiKey(clientId);

                // Assert
                var okResult = Assert.IsType<OkObjectResult>(result);
                var value = okResult.Value;

                Assert.NotNull(value);
                var apiKeyResponse = value.GetType().GetProperty("apiKey")?.GetValue(value, null);

                Assert.NotNull(apiKeyResponse);
                Assert.Equal("testapikey", ((ApiKey)apiKeyResponse).Key);
            }


            [Fact]
            public async Task GenerateApiKey_InvalidClientId_ReturnsBadRequest()
            {
                // Arrange
                string clientId = "";
                _mockApiKeyService.Setup(x => x.GenerateApiKeyAsync(clientId))
                                  .ThrowsAsync(new ArgumentException("ClientId is required."));

                // Act
                var result = await _controller.GenerateApiKey(clientId);

                // Assert
                Assert.IsType<BadRequestObjectResult>(result);
            }

            [Fact]
            public async Task ValidateApiKey_ValidApiKey_ReturnsOkResult()
            {
                // Arrange
                var loginRequest = new LoginRequest { ApiKey = "validKey" };
                _mockApiKeyService.Setup(x => x.ValidateApiKeyAsync(loginRequest.ApiKey)).ReturnsAsync(true);

                // Act
                var result = await _controller.ValidateApiKey(loginRequest);

                // Assert
                Assert.IsType<OkObjectResult>(result);
            }

            [Fact]
            public async Task ValidateApiKey_InvalidApiKey_ReturnsNotFoundResult()
            {
                // Arrange
                var loginRequest = new LoginRequest { ApiKey = "invalidKey" };
                _mockApiKeyService.Setup(x => x.ValidateApiKeyAsync(loginRequest.ApiKey)).ReturnsAsync(false);

                // Act
                var result = await _controller.ValidateApiKey(loginRequest);

                // Assert
                Assert.IsType<NotFoundObjectResult>(result);
            }
        }

        public class UserControllerTests
        {
            private readonly Mock<IUserService> _mockUserService;
            private readonly UserController _controller;

            public UserControllerTests()
            {
                _mockUserService = new Mock<IUserService>();
                _controller = new UserController(_mockUserService.Object);

                // Mock ControllerContext for ControllerBase dependencies
                _controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                };
            }

            [Fact]
            public async Task GetUsers_ReturnsOkResultWithUsers()
            {
                // Arrange
                var users = new List<UserResponseDto> {
                new UserResponseDto { Id = Guid.NewGuid(), UserName = "user1" },
                new UserResponseDto { Id = Guid.NewGuid(), UserName = "user2" }
                };

                _mockUserService.Setup(x => x.GetAllUsersAsync()).ReturnsAsync(users);

                // Act
                var result = await _controller.GetUsers();

                // Assert
                var okResult = Assert.IsType<OkObjectResult>(result);
                var returnedUsers = Assert.IsAssignableFrom<IEnumerable<UserResponseDto>>(okResult.Value);
                Assert.Equal(2, returnedUsers.Count());
            }

            [Fact]
            public async Task GetUser_ReturnsOkResultWithUser()
            {
                // Arrange
                var userId = Guid.NewGuid();
                var user = new UserResponseDto { Id = userId, UserName = "testuser" };
                _mockUserService.Setup(x => x.GetUserByIdAsync(userId)).ReturnsAsync(user);

                // Act
                var result = await _controller.GetUser(userId);

                // Assert
                var okResult = Assert.IsType<OkObjectResult>(result);
                var returnedUser = Assert.IsType<UserResponseDto>(okResult.Value);
                Assert.Equal(userId, returnedUser.Id);
                Assert.Equal("testuser", returnedUser.UserName);
            }

            [Fact]
            public async Task PostUser_ValidUser_ReturnsCreatedAtActionResult()
            {
                // Arrange
                var createUserDto = new CreateUserDto { UserName = "newuser", FullName = "New User",
                    Email = "new@example.com", MobileNumber = "1234567890", Language = "en",
                    Culture = "en-US", PasswordHash = "password" };

                var newUser = new User { Id = Guid.NewGuid(), UserName = "newuser", FullName = "New User",
                    Email = "new@example.com", MobileNumber = "1234567890", Language = "en",
                    Culture = "en-US", PasswordHash = "password" };

                // Hash the password before mocking the service call
                string passwordHash = Utilities.PasswordHasher.HashPassword(createUserDto.PasswordHash);

                _mockUserService.Setup(x => x.CreateUserAsync(createUserDto)).ReturnsAsync(newUser);

                // Act
                var result = await _controller.PostUser(createUserDto);

                // Assert
                var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
                Assert.Equal(nameof(UserController.PostUser), createdAtActionResult.ActionName);
                Assert.Equal(newUser, createdAtActionResult.Value);
            }

            [Fact]
            public async Task PutUser_ExistingUser_ReturnsOkResult()
            {
                // Arrange
                var userId = Guid.NewGuid();
                var updateUserDto = new UpdateUserDto { FullName = "Updated User" };
                var updatedUser = new User { Id = userId, UserName = "newuser", FullName = "New User",
                    Email = "new@example.com", MobileNumber = "1234567890", Language = "en",
                    Culture = "en-US", PasswordHash = "password" };

                _mockUserService.Setup(x => x.UpdateUserAsync(userId, updateUserDto)).ReturnsAsync(updatedUser);

                // Act
                var result = await _controller.PutUser(userId, updateUserDto);

                // Assert
                Assert.IsType<OkObjectResult>(result);
            }

            [Fact]
            public async Task DeleteUser_ExistingUser_ReturnsOkResult()
            {
                // Arrange
                var userId = Guid.NewGuid();
                _mockUserService.Setup(x => x.DeleteUserAsync(userId)).Returns(Task.CompletedTask);

                // Act
                var result = await _controller.DeleteUser(userId);

                // Assert
                Assert.IsType<OkObjectResult>(result);
            }

            [Fact]
            public async Task ValidatePassword_ValidCredentials_ReturnsOkResult()
            {
                // Arrange
                var request = new ValidatePasswordRequest { UserName = "testuser", Password = "password" };
                _mockUserService.Setup(x => x.ValidatePasswordAsync(request.UserName, request.Password)).ReturnsAsync(true);

                // Act
                var result = await _controller.ValidatePassword(request);

                // Assert
                Assert.IsType<OkObjectResult>(result);
            }

            [Fact]
            public async Task ValidatePassword_InvalidCredentials_ReturnsUnauthorizedResult()
            {
                // Arrange
                var request = new ValidatePasswordRequest { UserName = "testuser", Password = "wrongpassword" };
                _mockUserService.Setup(x => x.ValidatePasswordAsync(request.UserName, request.Password)).ReturnsAsync(false);

                // Act
                var result = await _controller.ValidatePassword(request);

                // Assert
                Assert.IsType<UnauthorizedObjectResult>(result);
            }
        }
    }
}