using API.DTOs;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;
[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{

private readonly IAccountService _accountService;

public  AccountController(IAccountService accountService)
{
    _accountService = accountService;
}

    [HttpPost("register")] // account/register
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        try
        {
           var userDto = await _accountService.RegisterAsync(registerDto);
           if (userDto == null) return BadRequest("User already exists");
           return userDto;
        }
        catch (Exception ex)
        {
            
            return StatusCode(500, $"Error registering user: {ex.Message}");
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
       try
       {
              var userDto = await _accountService.LoginAsync(loginDto);
              if (userDto == null) return Unauthorized("Invalid username or password");
              return userDto;
       }
       catch (Exception ex)
       {
        
       return StatusCode(500, $"Error logging in user: {ex.Message}");

       }
    }

    private async Task<bool> UserExists(string username)
    {
        try
        {
            return await _accountService.UserExistsAsync(username);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error checking if user exists: {ex.Message}");
        }
    }
}
