using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

public class AccountService : IAccountService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;

    private readonly ILogger<AccountService> _logger;

    public AccountService(UserManager<AppUser> userManager, ITokenService tokenService,
     IMapper mapper, ILogger<AccountService> logger)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _mapper = mapper;
        _logger = logger;
    }


    public async Task<UserDto> LoginAsync(LoginDto loginDto)
    {
        _logger.LogDebug("LoginAsync for user: {username}", loginDto.Username);
        var user = await _userManager.Users
            .Include(p => p.Photos)
                .FirstOrDefaultAsync(x =>
                    x.NormalizedUserName == loginDto.Username.ToUpper());

        if (user == null) throw new Exception("User is not found");

        _logger.LogInformation("User logged in successfully: {Username}", loginDto.Username);


        return new UserDto
        {
            Username = user.UserName!,
            KnownAs = user.KnownAs,
            Token = await _tokenService.CreateToken(user),
            Gender = user.Gender,
            PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url
        };
    }

    public async Task<UserDto> RegisterAsync(RegisterDto registerDto)
    {
        _logger.LogDebug("RegisterAsync for user: {Username}", registerDto.Username);

        if (await UserExistsAsync(registerDto.Username))
        {

            throw new Exception("Failed to register, user already exists");
        }

        var user = _mapper.Map<AppUser>(registerDto);

        user.UserName = registerDto.Username.ToLower();

        var result = await _userManager.CreateAsync(user, registerDto.Password);

        if (!result.Succeeded)
            throw new Exception("Failed creating user");

        _logger.LogInformation("User registered successfully: {Username}", registerDto.Username);

        return new UserDto
        {
            Username = user.UserName,
            Token = await _tokenService.CreateToken(user),
            KnownAs = user.KnownAs,
            Gender = user.Gender
        };
    }

    public Task<bool> UserExistsAsync(string username)
    {
        return _userManager.Users.AnyAsync(x => x.NormalizedUserName == username.ToUpper());
    }
}


