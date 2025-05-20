using API.DTOs;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{

    private readonly IAccountService _accountService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(IAccountService accountService, ILogger<AccountController> logger)
    {
        _accountService = accountService;
        _logger = logger;
    }

    /// <summary>
    /// POST /api/account/register
    /// </summary>
    /// <param name="registerDto"></param>
    /// <returns></returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ActionResult<UserDto?>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        try
        {
            _logger.LogDebug($"AccountController - {nameof(Register)} invoked.(registerDto: {registerDto})");
            await _accountService.RegisterAsync(registerDto);
            return Ok(registerDto);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in AccountController.Register");
            throw;
        }
    }

    /// <summary>
    /// POST /api/account/login
    /// </summary>
    /// <param name="loginDto"></param>
    /// <returns></returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ActionResult<UserDto?>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        try
        {
            _logger.LogDebug($"AccontController - {nameof(Login)} invoked. (loginDto: {loginDto})");
            var userDto = await _accountService.LoginAsync(loginDto);
            return Ok(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in AccountController.Login");
            throw;

        }
    }

}
