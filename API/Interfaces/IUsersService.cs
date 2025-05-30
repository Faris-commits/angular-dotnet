using API.DTOs;
using API.Helpers;

namespace API.Interfaces;

public interface IUsersService
{

    Task<PagedList<MemberDto>> GetUsersAsync(UserParams userParams, string currentUsername);
    Task<MemberDto> GetUserAsync(string username, string currentUsername);

    Task UpdateUserAsync(string username, MemberUpdateDto memberUpdateDto);

    Task<bool> SetMainPhotoAsync(string username, int photoId);

    Task<bool> DeletePhotoAsync(string username, int photoId);

    Task<PhotoDto> AddPhotoAsync(string username, IFormFile file);

    Task SetPhotoTagsAsync(string username, int photoId, List<int> tagIds);
    Task<bool> DeleteTagAsync(int tagId);
    Task<IEnumerable<PhotoDto>> GetPhotosByTagAsync(int tagId);

}
