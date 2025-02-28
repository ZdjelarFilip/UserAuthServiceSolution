using Newtonsoft.Json;
using System.Net;
using UserAuthService.Data;
using UserAuthService.Services;

public class ApiKeyValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IServiceProvider _services;

    public ApiKeyValidationMiddleware(RequestDelegate next, IServiceProvider services)
    {
        _next = next;
        _services = services;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        using var scope = _services.CreateScope();
        var apiKeyService = scope.ServiceProvider.GetRequiredService<IApiKeyService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();

        // Allow Swagger & OpenAPI to bypass authentication
        if (context.Request.Path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        // Check for API key in headers
        if (!context.Request.Headers.TryGetValue("X-API-Key", out var providedApiKey))
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

            // Create a JSON object for the error response
            var errorResponse = new
            {
                StatusCode = (int)HttpStatusCode.Unauthorized,
                Message = "API Key is missing."
            };

            // Set the content type to application/json and write the JSON response
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonConvert.SerializeObject(errorResponse));
            return;
        }

        // Validate API key using the injected IApiKeyService
        var isValidApiKey = await apiKeyService.ValidateApiKeyAsync(providedApiKey.ToString());

        if (!isValidApiKey)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;

            // Create a JSON object for the error response
            var errorResponse = new
            {
                StatusCode = (int)HttpStatusCode.Forbidden,
                Message = "Invalid API Key."
            };

            // Set the content type to application/json and write the JSON response
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonConvert.SerializeObject(errorResponse));
            return;
        }

        // Continue processing the request
        await _next(context);
    }
}
