using Microsoft.EntityFrameworkCore;
using UserAuthService.Utilities;
using UserAuthService.Data;
using UserAuthService.Models.Auth;

namespace UserAuthService.Services
{
    public class ApiKeyService : IApiKeyService
    {
        private readonly UserDbContext _dbContext;

        public ApiKeyService(UserDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ApiKey> GenerateApiKeyAsync(string clientId)
        {
            if (string.IsNullOrEmpty(clientId))
                throw new ArgumentException("ClientId is required.");

            if (await _dbContext.ApiKeys.AnyAsync(u => u.ClientId == clientId))
                throw new ArgumentException("API key already exists for this ClientId:", clientId);

            var apiKey = new ApiKey
            {
                Id = Guid.NewGuid(),
                Key = ApiKeyGenerator.GenerateSecureApiKey(),
                ClientId = clientId,
                ValidUntil = DateTime.UtcNow.AddDays(20)
            };

            _dbContext.ApiKeys.Add(apiKey);
            await _dbContext.SaveChangesAsync();

            return apiKey;
        }

        public async Task<bool> ValidateApiKeyAsync(string apiKey)
        {
            var apiKeyEntity = await _dbContext.ApiKeys.FirstOrDefaultAsync(u => u.Key == apiKey);

            if (apiKeyEntity == null || apiKeyEntity.ValidUntil < DateTime.UtcNow)
            {
                return false;
            }
            else
            {
                return true;
            }
         }
    }
}
