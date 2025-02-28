using UserAuthService.Models.Auth;

namespace UserAuthService.Services
{
    public interface IApiKeyService
    {
        Task<ApiKey> GenerateApiKeyAsync(string clientId);
        Task<bool> ValidateApiKeyAsync(string apiKey);
    }
}
