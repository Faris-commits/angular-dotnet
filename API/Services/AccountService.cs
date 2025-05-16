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

    public AccountService(UserManager<AppUser> userManager, ITokenService tokenService, IMapper mapper)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _mapper = mapper;
    }


    public async Task<UserDto> LoginAsync(LoginDto loginDto)
    {
        var user = await _userManager.Users
            .Include(p => p.Photos)
                .FirstOrDefaultAsync(x =>
                    x.NormalizedUserName == loginDto.Username.ToUpper());

        if (user == null || user.UserName == null) return null;

        return new UserDto
        {
            Username = user.UserName,
            KnownAs = user.KnownAs,
            Token = await _tokenService.CreateToken(user),
            Gender = user.Gender,
            PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url
        };
    }

    public async Task<UserDto> RegisterAsync(RegisterDto registerDto)
    {
        if (await UserExistsAsync(registerDto.Username)) return null;

        var user = _mapper.Map<AppUser>(registerDto);

        user.UserName = registerDto.Username.ToLower();

        var result = await _userManager.CreateAsync(user, registerDto.Password);

        if (!result.Succeeded) throw new Exception("Failed creating user");

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


