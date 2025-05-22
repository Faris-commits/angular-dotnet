using API.DTOs;
using API.Interfaces;
using Serilog;
using ILogger = Serilog.ILogger;

namespace API.Services;

public class AdminService : IAdminService
{
    private readonly IUnitOfWork unitOfWork;
    private readonly IPhotoService photoService;
    private readonly ILogger _logger;

    public AdminService(IUnitOfWork unitOfWork, IPhotoService photoService, ILogger logger)
    {
        this.unitOfWork = unitOfWork;
        this.photoService = photoService;
        _logger = logger;
    }

    public async Task<(bool Success, string Message)> ApprovePhotoAsync(int photoId)
    {
        var photo = await unitOfWork.PhotoRepository.GetPhotoById(photoId);
        if (photo == null) return (false, "Photo not found.");

        photo.IsApproved = true;

        var user = await unitOfWork.UserRepository.GetUserByPhotoId(photoId);
        if (user == null) return (false, "User not found.");

        if (!user.Photos.Any(x => x.IsMain)) photo.IsMain = true;

        var success = await unitOfWork.Complete();
        if (success)
        {
            _logger.Information("Photo with ID {PhotoId} has been approved by the admin.", photoId);
            return (true, "Photo approved successfully.");
        }

        return (false, "Failed to approve photo.");
    }


    public async Task<(bool Success, string Message)> RejectPhotoAsync(int photoId)
    {
        var photo = await unitOfWork.PhotoRepository.GetPhotoById(photoId);
        if (photo == null) return (false, "Photo not found.");

        if (photo.PublicId != null)
        {
            var result = await photoService.DeletePhotoAsync(photo.PublicId);
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                unitOfWork.PhotoRepository.RemovePhoto(photo);
            }
        }
        else
        {
            unitOfWork.PhotoRepository.RemovePhoto(photo);
        }

        var success = await unitOfWork.Complete();
        if (success)
        {
            _logger.Information("Photo with ID {PhotoId} has been rejected and deleted by the admin.", photoId);
            return (true, "Photo rejected and deleted successfully.");
        }

        return (false, "Failed to reject photo.");
    }

    public async Task<(bool Success, IEnumerable<string> Roles)> EditRolesAsync(string username, string roles)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be null or empty.", nameof(username));

        if (string.IsNullOrWhiteSpace(roles))
            throw new ArgumentException("Roles cannot be null or empty.", nameof(roles));

        var selectedRoles = roles.Split(",").ToArray();
        if (!selectedRoles.Any())
            throw new ArgumentException("At least one role must be selected.", nameof(roles));

        var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(username);
        if (user == null)
            throw new KeyNotFoundException($"User with username '{username}' not found.");

        var currentRoles = await unitOfWork.UserRepository.GetUserRolesAsync(user);

        var rolesToAdd = selectedRoles.Except(currentRoles).ToArray();
        var addResult = await unitOfWork.UserRepository.AddToRolesAsync(user, rolesToAdd);
        if (!addResult)
            throw new InvalidOperationException("Failed to add roles to the user.");

        var rolesToRemove = currentRoles.Except(selectedRoles).ToArray();
        var removeResult = await unitOfWork.UserRepository.RemoveFromRolesAsync(user, rolesToRemove);
        if (!removeResult)
            throw new InvalidOperationException("Failed to remove roles from the user.");

        var updatedRoles = await unitOfWork.UserRepository.GetUserRolesAsync(user);
        return (true, updatedRoles);
    }


    public async Task<(IEnumerable<PhotoForApprovalDto> Photos, string Message)> GetPhotosForModerationAsync()
    {
        var photos = await unitOfWork.PhotoRepository.GetUnapprovedPhotos();

        if (photos == null || !photos.Any())
        {
            return (Enumerable.Empty<PhotoForApprovalDto>(), "No photos need moderation at this time.");
        }

        return (photos, "Photos retrieved successfully.");
    }

    public async Task<IEnumerable<object>> GetUsersWithRolesAsync()
    {
        return await unitOfWork.UserRepository.GetUsersWithRolesAsync();
    }
}