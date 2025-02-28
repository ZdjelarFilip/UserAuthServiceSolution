using UserAuthService.DTOs;
using UserAuthService.Models.Auth;

namespace UserAuthService.Services
{
    public interface IUserService
    {
        Task<IEnumerable<UserResponseDto?>> GetAllUsersAsync();
        Task<UserResponseDto?> GetUserByIdAsync(Guid id);
        Task<User> CreateUserAsync(CreateUserDto user);
        Task<User> UpdateUserAsync(Guid id, UpdateUserDto user);
        Task DeleteUserAsync(Guid id);
        Task<bool> ValidatePasswordAsync(string userName, string password);
    }
}
