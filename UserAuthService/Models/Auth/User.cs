using Microsoft.EntityFrameworkCore;

namespace UserAuthService.Models.Auth
{
    [Index(nameof(UserName), IsUnique = true)]
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string UserName { get; set; }
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public required string MobileNumber { get; set; }
        public required string Language { get; set; }
        public required string Culture { get; set; }
        public required string PasswordHash { get; set; }
    }
}
