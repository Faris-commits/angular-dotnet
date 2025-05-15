using API.DTOs;
using API.Interfaces;

namespace API.Services;

public class AdminService : IAdminService
{
    private readonly IUnitOfWork unitOfWork;
    private readonly IPhotoService photoService;

    public AdminService(IUnitOfWork unitOfWork, IPhotoService photoService)
    {
        this.unitOfWork = unitOfWork;
        this.photoService = photoService;
    }

    public async Task<bool> ApprovePhotoAsync(int photoId)
    {
        var photo = await unitOfWork.PhotoRepository.GetPhotoById(photoId);
        if (photo == null) return false;

        photo.IsApproved = true;

        var user = await unitOfWork.UserRepository.GetUserByPhotoId(photoId);
        if (user == null) return false;

        if (!user.Photos.Any(x => x.IsMain)) photo.IsMain = true;

        return await unitOfWork.Complete();
    }

    public async Task<bool> RejectPhotoAsync(int photoId)
    {
        var photo = await unitOfWork.PhotoRepository.GetPhotoById(photoId);
        if (photo == null) return false;

        if (photo.PublicId != null)
        {
            var result = await photoService.DeletePhotoAsync(photo.PublicId);
            if (result.Result == "ok")
            {
                unitOfWork.PhotoRepository.RemovePhoto(photo);
            }
        }
        else
        {
            unitOfWork.PhotoRepository.RemovePhoto(photo);
        }

        return await unitOfWork.Complete();
    }

    public async Task<(bool Success, string ErrorMessage, IEnumerable<string> Roles)> EditRolesAsync(string username, string roles)
    {
        if (string.IsNullOrEmpty(roles)) return (false, "You must select at least one role", null);

        var selectedRoles = roles.Split(",").ToArray();
        var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(username);

        if (user == null) return (false, "User not found", null);

        var userRoles = await unitOfWork.UserRepository.GetUserRolesAsync(user);

        var addResult = await unitOfWork.UserRepository.AddToRolesAsync(user, selectedRoles.Except(userRoles));
        if (!addResult) return (false, "Failed to add to roles", null);

        var removeResult = await unitOfWork.UserRepository.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));
        if (!removeResult) return (false, "Failed to remove from roles", null);

        var updatedRoles = await unitOfWork.UserRepository.GetUserRolesAsync(user);
        return (true, null, updatedRoles);
    }

    public async Task<IEnumerable<PhotoForApprovalDto>> GetPhotosForModerationAsync()
    {
        return await unitOfWork.PhotoRepository.GetUnapprovedPhotos();
    }

    public async Task<IEnumerable<object>> GetUsersWithRolesAsync()
    {
        return await unitOfWork.UserRepository.GetUsersWithRolesAsync();
    }
}