using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace API.Services;

public class AdminService : IAdminService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPhotoService _photoService;
    private readonly ILogger<AdminService> _logger;
    private readonly IMapper _mapper;

    public AdminService(IUnitOfWork unitOfWork, IPhotoService photoService, ILogger<AdminService> logger, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _photoService = photoService;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<(bool Success, string Message)> ApprovePhotoAsync(int photoId)
    {
        var photo = await _unitOfWork.PhotoRepository.GetPhotoById(photoId);
        if (photo == null) return (false, "Photo not found.");

        photo.IsApproved = true;

        var user = await _unitOfWork.UserRepository.GetUserByPhotoId(photoId);
        if (user == null) return (false, "User not found.");

        if (!user.Photos.Any(x => x.IsMain)) photo.IsMain = true;

        var success = await _unitOfWork.Complete();
        if (success)
        {
            _logger.LogInformation("Photo with ID {PhotoId} has been approved by the admin.", photoId);
            return (true, "Photo approved successfully.");
        }

        return (false, "Failed to approve photo.");
    }

    public async Task<(bool Success, string Message)> RejectPhotoAsync(int photoId)
    {
        var photo = await _unitOfWork.PhotoRepository.GetPhotoById(photoId);
        if (photo == null) return (false, "Photo not found.");

        if (photo.PublicId != null)
        {
            var result = await _photoService.DeletePhotoAsync(photo.PublicId);
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                _unitOfWork.PhotoRepository.RemovePhoto(photo);
            }
        }
        else
        {
            _unitOfWork.PhotoRepository.RemovePhoto(photo);
        }

        var success = await _unitOfWork.Complete();
        if (success)
        {
            _logger.LogInformation("Photo with ID {PhotoId} has been rejected and deleted by the admin.", photoId);
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

        var selectedRoles = roles.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(r => r.Trim()).ToArray();
        if (!selectedRoles.Any())
            throw new ArgumentException("At least one role must be selected.", nameof(roles));

        var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);
        if (user == null)
            throw new KeyNotFoundException($"User with username '{username}' not found.");

        var currentRoles = await _unitOfWork.UserRepository.GetUserRolesAsync(user);

        var rolesToAdd = selectedRoles.Except(currentRoles).ToArray();
        var addResult = await _unitOfWork.UserRepository.AddToRolesAsync(user, rolesToAdd);
        if (!addResult)
            throw new InvalidOperationException("Failed to add roles to the user.");

        var rolesToRemove = currentRoles.Except(selectedRoles).ToArray();
        var removeResult = await _unitOfWork.UserRepository.RemoveFromRolesAsync(user, rolesToRemove);
        if (!removeResult)
            throw new InvalidOperationException("Failed to remove roles from the user.");

        var updatedRoles = await _unitOfWork.UserRepository.GetUserRolesAsync(user);
        return (true, updatedRoles);
    }

    public async Task<(IEnumerable<PhotoForApprovalDto> Photos, string Message)> GetPhotosForModerationAsync()
    {
        var photos = await _unitOfWork.PhotoRepository.GetUnapprovedPhotos();

        if (photos == null || !photos.Any())
        {
            return (Enumerable.Empty<PhotoForApprovalDto>(), "No photos need moderation at this time.");
        }

        return (photos, "Photos retrieved successfully.");
    }

    public async Task<IEnumerable<object>> GetUsersWithRolesAsync()
    {
        return await _unitOfWork.UserRepository.GetUsersWithRolesAsync();
    }

    public async Task<IEnumerable<PhotoTagDto>> GetPhotoTagsAsync()
    {
        var tags = await _unitOfWork.TagRepository.GetAllTagsAsync();
        return tags.Select(t => _mapper.Map<PhotoTagDto>(t));
    }

    public async Task<PhotoTagDto> CreatePhotoTagAsync(string tagName)
    {
        if (string.IsNullOrWhiteSpace(tagName))
            throw new ArgumentException("Tag name cannot be empty.");

        var existing = await _unitOfWork.TagRepository.GetTagByNameAsync(tagName.Trim());
        if (existing != null)
            throw new InvalidOperationException("Tag already exists.");

        var tag = new Tag { Name = tagName.Trim() };
        await _unitOfWork.TagRepository.AddTagAsync(tag);
        await _unitOfWork.TagRepository.SaveAllAsync();
        return _mapper.Map<PhotoTagDto>(tag);
    }

    public async Task<bool> DeletePhotoTagAsync(int tagId)
    {
        var tag = await _unitOfWork.TagRepository.GetTagByIdAsync(tagId);
        if (tag == null) return false;
        _unitOfWork.TagRepository.RemoveTag(tag);
        return await _unitOfWork.TagRepository.SaveAllAsync();
    }
}