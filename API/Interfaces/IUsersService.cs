using API.DTOs;
using API.Helpers;

namespace API.Interfaces;

public interface IUsersService
{

    Task<PagedList<MemberDto>> GetUsersAsync (UserParams userParams, string currentUsername);
    Task<MemberDto> GetUserAsync(string username, string currentUsername);

    Task<bool> UpdateUserAsync(string currentUsername, MemberUpdateDto dto);

    Task<bool> SetMainPhotoAsync(string username, int photoId);

    Task<bool> DeletePhotoAsync(string username, int photoId);

    Task<PhotoDto> AddPhotoAsync(string username, IFormFile file);

}
