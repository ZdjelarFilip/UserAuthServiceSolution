using Microsoft.AspNetCore.Mvc;
using UserAuthService.Models.Auth;
using UserAuthService.Services;

[ApiController]
[Route("api/[controller]/[action]")]
[Consumes("application/json")]
[Produces("application/json")]
public class ApiKeysController : ControllerBase
{
    private readonly IApiKeyService _apiKeyService;

    public ApiKeysController(IApiKeyService apiKeyService)
    {
        _apiKeyService = apiKeyService;
    }

    [HttpPost(Name = "GenerateApiKey")]
    public async Task<IActionResult> GenerateApiKey([FromQuery] string clientId)
    {
        try
        {
            var apiKey = await _apiKeyService.GenerateApiKeyAsync(clientId);
            return Ok(new {apiKey});
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while generating the API key.");
        }
    }

    [HttpPost(Name = "ValidateApiKey")]
    public async Task<IActionResult> ValidateApiKey([FromQuery] LoginRequest request)
    {
        try
        {
            var isValid = await _apiKeyService.ValidateApiKeyAsync(request.ApiKey);

            if (isValid)
            {
                return Ok(new { Message = "Validation successful." });
            }
            else
            {
                return NotFound(new { Message = "Validation unsuccessful - Invalid API Key." });
            }
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message, StatusCode = 400 });
        }
        catch (Exception)
        {
            return StatusCode(500, new { Message = "An error occurred while validating the API key." });
        }
    }
}