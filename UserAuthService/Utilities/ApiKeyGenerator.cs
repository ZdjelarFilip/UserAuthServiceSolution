using System.Security.Cryptography;

namespace UserAuthService.Utilities
{
    public class ApiKeyGenerator
    {
        private const int KeyLength = 32;

        public static string GenerateSecureApiKey()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var bytes = new byte[KeyLength];
                rng.GetBytes(bytes);
                return BitConverter.ToString(bytes).Replace("-", "").ToLower(); // Hexadecimal encoding (lowercase)
            }
        }
    }
}