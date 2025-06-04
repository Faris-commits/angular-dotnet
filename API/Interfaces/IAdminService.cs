using API.DTOs;

namespace API.Interfaces;

public interface IAdminService
{
    Task<(bool Success, string Message)> ApprovePhotoAsync(int photoId);
    Task<(bool Success, string Message)> RejectPhotoAsync(int photoId, string reason);
    Task<IEnumerable<object>> GetUsersWithRolesAsync();
    Task<IEnumerable<PhotoTagDto>> GetPhotoTagsAsync();
    Task<PhotoTagDto> CreatePhotoTagAsync(string tagName);
    Task<PhotoTagDto?> AddTagToPhotoAsync(int photoId, int tagId);
    Task<bool> RemoveTagFromPhotoAsync(int photoId, int tagId);
    Task<bool> DeletePhotoTagAsync(int tagId);
    Task<(bool Success, IEnumerable<string> Roles)> EditRolesAsync(string username, string roles);
    Task<(IEnumerable<PhotoForApprovalDto> Photos, string Message)> GetPhotosForModerationAsync();
    Task<IEnumerable<PhotoApprovalStatsDto>> GetPhotoApprovalStatsAsync();
    Task<IEnumerable<string>> GetUsersWithoutMainPhotoAsync();
}