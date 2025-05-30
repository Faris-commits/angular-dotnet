using API.Data;
using API.DTOs;
using API.Entities;
using API.Helpers;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API;

public class UserRepository : IUserRepository
{
    private readonly DataContext context;
    private readonly IMapper mapper;
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;

    public UserRepository(DataContext context, IMapper mapper, UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
    {
        this.context = context;
        this.mapper = mapper;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<bool> AddToRolesAsync(AppUser user, IEnumerable<string> roles)
    {
        var result = await _userManager.AddToRolesAsync(user, roles);
        return result.Succeeded;
    }

    public async Task<MemberDto?> GetMemberAsync(string username)
    {
        return await context.Users
            .Where(x => x.UserName == username)
            .ProjectTo<MemberDto>(mapper.ConfigurationProvider)
            .SingleOrDefaultAsync();
    }

    public async Task<MemberDto> GetMemberAsync(string username, bool isCurrentUser)
    {
        var query = context.Users
            .Where(x => x.UserName == username)
            .ProjectTo<MemberDto>(mapper.ConfigurationProvider)
            .AsQueryable();

        if (isCurrentUser) query = query.IgnoreQueryFilters();
        return await query.FirstOrDefaultAsync();
    }

    public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
    {
        var query = context.Users.AsQueryable();

        query = query.Where(x => x.UserName != userParams.CurrentUsername);

        if (userParams.Gender != null)
        {
            query = query.Where(x => x.Gender == userParams.Gender);
        }

        var minDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MaxAge - 1));
        var maxDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MinAge));

        query = query.Where(x => x.DateOfBirth >= minDob && x.DateOfBirth <= maxDob);

        query = userParams.OrderBy switch
        {
            "created" => query.OrderByDescending(x => x.Created),
            _ => query.OrderByDescending(x => x.LastActive)
        };

        return await PagedList<MemberDto>.CreateAsync(query.ProjectTo<MemberDto>(mapper.ConfigurationProvider),
            userParams.PageNumber, userParams.PageSize);

    }

    public async Task<AppUser?> GetUserByIdAsync(int id)
    {
        return await context.Users.FindAsync(id);
    }

    public async Task<AppUser?> GetUserByPhotoId(int photoId)
    {
        return await context.Users
            .Include(x => x.Photos)
            .IgnoreQueryFilters()
            .Where(x => x.Photos.Any(p => p.Id == photoId))
            .FirstOrDefaultAsync();
    }

    public async Task<AppUser> GetUserByUsernameAsync(string username)
    {
        return await context.Users
            .Include(u => u.Photos)
                .ThenInclude(p => p.PhotoTags)
                    .ThenInclude(pt => pt.Tag)
            .SingleOrDefaultAsync(x => x.UserName == username);
    }

    public async Task<IEnumerable<string>> GetUserRolesAsync(AppUser user)
    {
        return await _userManager.GetRolesAsync(user);
    }

    public async Task<IEnumerable<AppUser>> GetUsersAsync()
    {
        return await context.Users
            .Include(x => x.Photos)
            .ToListAsync();
    }

    public async Task<IEnumerable<object>> GetUsersWithRolesAsync()
    {
        return await context.Users
            .OrderBy(u => u.UserName)
            .Select(u => new
            {
                u.Id,
                Username = u.UserName,
                Roles = (from userRole in context.UserRoles
                         join role in context.Roles on userRole.RoleId equals role.Id
                         where userRole.UserId == u.Id
                         select role.Name).ToList()
            })
            .ToListAsync();
    }

    public async Task<bool> RemoveFromRolesAsync(AppUser user, IEnumerable<string> roles)
    {
        var result = await _userManager.RemoveFromRolesAsync(user, roles);
        return result.Succeeded;
    }

    public void Update(AppUser user)
    {
        context.Entry(user).State = EntityState.Modified;
    }
}