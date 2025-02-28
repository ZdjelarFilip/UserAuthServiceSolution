using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using UserAuthService.Utilities;
using UserAuthService.Data;
using UserAuthService.DTOs;
using UserAuthService.Models.Auth;

namespace UserAuthService.Services
{
    public class UserService : IUserService
    {
        private readonly UserDbContext _dbContext;

        public UserService(UserDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<UserResponseDto?>> GetAllUsersAsync()
        {
            var users = await _dbContext.Users.ToListAsync();

            // Map each User to UserResponseDto without Password
            return users.Select(user => new UserResponseDto
            {
                Id = user.Id,
                UserName = user.UserName,
                FullName = user.FullName,
                Email = user.Email,
                MobileNumber = user.MobileNumber,
                Language = user.Language,
                Culture = user.Culture
            });
        }

        public async Task<UserResponseDto?> GetUserByIdAsync(Guid id)
        {
            var user = await _dbContext.Users.FindAsync(id);

            if (user == null)
            {
                return null;
            }

            // Map User to UserResponseDto without Password
            return new UserResponseDto
            {
                Id = user.Id,
                UserName = user.UserName,
                FullName = user.FullName,
                Email = user.Email,
                MobileNumber = user.MobileNumber,
                Language = user.Language,
                Culture = user.Culture
            };
        }

        public async Task<User> CreateUserAsync(CreateUserDto userDto)
        {
            if (await _dbContext.Users.AnyAsync(u => u.UserName == userDto.UserName))
            {
                throw new ArgumentException("User with this username already exists.");
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = userDto.UserName,
                FullName = userDto.FullName,
                Email = userDto.Email,
                MobileNumber = userDto.MobileNumber,
                Language = userDto.Language,
                Culture = userDto.Culture,
                PasswordHash = PasswordHasher.HashPassword(userDto.PasswordHash)
            };

            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            return user;
        }

        public async Task<User> UpdateUserAsync(Guid id, UpdateUserDto userDto)
        {
            var existingUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (existingUser == null)
            {
                throw new ArgumentException("User with this Id does not exist");
            }

            // Check if the new username already exists (excluding the current user)
            var usernameExists = await _dbContext.Users
                .AnyAsync(u => u.UserName == userDto.UserName && u.Id != id);

            if (usernameExists)
            {
                throw new ArgumentException("This username is already taken.");
            }

            // Update existing user
            existingUser.UserName = userDto.UserName;
            existingUser.FullName = userDto.FullName;
            existingUser.Email = userDto.Email;
            existingUser.MobileNumber = userDto.MobileNumber;
            existingUser.Language = userDto.Language;
            existingUser.Culture = userDto.Culture;

            // Only update the password if provided
            if (!string.IsNullOrWhiteSpace(userDto.PasswordHash))
            {
                existingUser.PasswordHash = PasswordHasher.HashPassword(userDto.PasswordHash);
            }

            _dbContext.Users.Update(existingUser);
            await _dbContext.SaveChangesAsync();

            return existingUser;
        }

        public async Task DeleteUserAsync(Guid id)
        {
            var user = await _dbContext.Users.FindAsync(id);

            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> ValidatePasswordAsync(string userName, string password)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == userName);

            if (user == null) return false;

            return VerifyPassword(password, user.PasswordHash);
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedInput = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(password)));
                return hashedInput == storedHash;
            }
        }
    }
}
