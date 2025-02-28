using Microsoft.AspNetCore.Mvc;
using UserAuthService.DTOs;
using UserAuthService.Models.Auth;
using UserAuthService.Services;

[ApiController]
[Route("api/[controller]/[action]")]
[Consumes("application/json")]
[Produces("application/json")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet(Name = "GetUsers")]
    public async Task<IActionResult> GetUsers()
    {
        try
        {
            var users = await _userService.GetAllUsersAsync();

            if (users == null || !users.Any())
            {
                return Ok(new { });
            }

            return Ok(users);
        }
        catch (Exception)
        {
            return StatusCode(500, new { Message = "An error occurred while retrieving users." });
        }
    }

    [HttpGet(Name = "GetUser")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id);

            if (user == null)
            {
                return Ok(new { });
            }

            return Ok(user);
        }
        catch (Exception)
        {
            return StatusCode(500, new { Message = "An error occurred while retrieving the user." });
        }
    }

    [HttpPost(Name = "PostUser")]
    public async Task<IActionResult> PostUser([FromBody] CreateUserDto user)
    {
        try
        {
            var newUser = await _userService.CreateUserAsync(user);
            return CreatedAtAction(nameof(PostUser), newUser);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { Message = "An error occurred while saving the data." });
        }
    }

    [HttpPut(Name = "PutUser")]
    public async Task<IActionResult> PutUser(Guid id, [FromBody] UpdateUserDto user)
    {
        try
        {
            var updatedUser = await _userService.UpdateUserAsync(id, user);
            return Ok(updatedUser);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpDelete(Name = "DeleteUser")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        try
        {
            await _userService.DeleteUserAsync(id);
            return Ok(new { Message = "User successfully deleted." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }

    [HttpPost(Name = "ValidatePassword")]
    public async Task<IActionResult> ValidatePassword([FromBody] ValidatePasswordRequest request)
    {
        try
        {
            bool isValid = await _userService.ValidatePasswordAsync(request.UserName, request.Password);
            
            if (isValid)
            {
                return Ok(new { Message = "Password is valid." });
            }
            else
            {
                return Unauthorized(new { Message = "Invalid credentials." });
            }
        }
        catch (Exception)
        {
            return StatusCode(500, new { Message = "An error occurred while validating the password." });
        }
    }
}