using API.DTOs;

namespace API.Interfaces;

public interface IAdminService
{
    Task<bool> ApprovePhotoAsync(int photoId);
    Task<bool> RejectPhotoAsync(int photoId);
    Task<IEnumerable<object>> GetUsersWithRolesAsync();
    Task<(bool Success, string ErrorMessage, IEnumerable<string> Roles)> EditRolesAsync(string username, string roles);
    Task<IEnumerable<PhotoForApprovalDto>> GetPhotosForModerationAsync();
}