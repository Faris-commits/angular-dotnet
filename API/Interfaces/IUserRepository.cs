using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API;

public interface IUserRepository
{
    void Update(AppUser user);
    Task<IEnumerable<AppUser>> GetUsersAsync();
    Task<AppUser?> GetUserByIdAsync(int id);
    Task<AppUser?> GetUserByUsernameAsync(string username);
    Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams);
    Task<MemberDto?> GetMemberAsync(string username);
    Task<MemberDto> GetMemberAsync(string username, bool isCurrentUser);
    Task<AppUser?> GetUserByPhotoId(int photoId);
    Task<IEnumerable<object>> GetUsersWithRolesAsync();
    Task<IEnumerable<string>> GetUserRolesAsync(AppUser user);
    Task<bool> AddToRolesAsync(AppUser user, IEnumerable<string> enumerable);
    Task<bool> RemoveFromRolesAsync(AppUser user, IEnumerable<string> enumerable);
    Task<IEnumerable<MatchDto>> GetMatchesForUserAsync(
        int userId,
        string? gender = null,
        string? city = null
    );
}
