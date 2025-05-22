using API.DTOs;

namespace API.Interfaces;

public interface IAdminService
{
    Task<(bool Success, string Message)> ApprovePhotoAsync(int photoId);
    Task<(bool Success, string Message)> RejectPhotoAsync(int photoId);
    Task<IEnumerable<object>> GetUsersWithRolesAsync();
    Task<(bool Success, IEnumerable<string> Roles)> EditRolesAsync(string username, string roles);
    Task<(IEnumerable<PhotoForApprovalDto> Photos, string Message)> GetPhotosForModerationAsync();
}