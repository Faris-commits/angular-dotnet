using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

public class AdminService : IAdminService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPhotoService _photoService;
    private readonly ILogger<AdminService> _logger;
    private readonly IMapper _mapper;
    private readonly DataContext _context;
    private readonly IMessageRepository _messageRepository;

    public AdminService(
        IUnitOfWork unitOfWork,
        IPhotoService photoService,
        ILogger<AdminService> logger,
        IMapper mapper,
        DataContext context,
        IMessageRepository messageRepository
    )
    {
        _unitOfWork = unitOfWork;
        _photoService = photoService;
        _logger = logger;
        _mapper = mapper;
        _context = context;
        _messageRepository = messageRepository;
    }

    public async Task<(bool Success, string Message)> ApprovePhotoAsync(int photoId)
    {
        if (photoId <= 0)
            throw new ArgumentException("Invalid photo ID.", nameof(photoId));

        var photo = await _unitOfWork.PhotoRepository.GetPhotoById(photoId);
        throw new KeyNotFoundException($"Photo with ID {photoId} not found.");

        if (photo.IsApproved)
            return (true, "Photo already approved.");

        var user = await _unitOfWork.UserRepository.GetUserByPhotoId(photoId);
        throw new KeyNotFoundException($"User associated with photo ID {photoId} not found.");

        photo.IsApproved = true;

        if (!user.Photos.Any(p => p.IsMain))
            photo.IsMain = true;

        var success = await _unitOfWork.Complete();
        if (!success)
            throw new InvalidOperationException(
                "Failed to save changes while approving the photo."
            );
    }

    public async Task<(bool Success, string Message)> RejectPhotoAsync(int photoId, string reason)
    {
        if (photoId <= 0)
            throw new ArgumentException("Invalid photo ID.", nameof(photoId));

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Rejection reason is required.", nameof(reason));

        var photo =
            await _unitOfWork.PhotoRepository.GetPhotoById(photoId)
            ?? throw new KeyNotFoundException($"Photo with ID {photoId} not found.");

        var user =
            await _unitOfWork.UserRepository.GetUserByPhotoId(photoId)
            ?? throw new KeyNotFoundException(
                $"User associated with photo ID {photoId} not found."
            );

        if (!string.IsNullOrEmpty(photo.PublicId))
        {
            var result = await _photoService.DeletePhotoAsync(photo.PublicId);
            if (result.StatusCode != System.Net.HttpStatusCode.OK)
                throw new InvalidOperationException(
                    "Failed to delete photo from external storage."
                );
        }

        _unitOfWork.PhotoRepository.RemovePhoto(photo);

        var adminUser =
            await _context.Users.FirstOrDefaultAsync(u => u.UserName == "admin")
            ?? throw new InvalidOperationException("Admin user not found.");

        var message = new Message
        {
            SenderId = adminUser.Id,
            SenderUsername = adminUser.UserName,
            RecipientId = user.Id,
            RecipientUsername = user.UserName,
            Content = $"Your photo was rejected by an admin. Reason: {reason}",
        };

        _messageRepository.AddMessage(message);

        var success = await _unitOfWork.Complete();
        if (!success)
            throw new InvalidOperationException(
                "Failed to save changes while rejecting the photo."
            );

        return (true, "Photo rejected and deleted successfully.");
    }

    public async Task<(bool Success, IEnumerable<string> Roles)> EditRolesAsync(
        string username,
        string roles
    )
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be null or empty.", nameof(username));

        if (string.IsNullOrWhiteSpace(roles))
            throw new ArgumentException("Roles cannot be null or empty.", nameof(roles));

        var selectedRoles = roles
            .Split(",", StringSplitOptions.RemoveEmptyEntries)
            .Select(r => r.Trim())
            .ToArray();
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
        var removeResult = await _unitOfWork.UserRepository.RemoveFromRolesAsync(
            user,
            rolesToRemove
        );
        if (!removeResult)
            throw new InvalidOperationException("Failed to remove roles from the user.");

        var updatedRoles = await _unitOfWork.UserRepository.GetUserRolesAsync(user);
        return (true, updatedRoles);
    }

    public async Task<(
        IEnumerable<PhotoForApprovalDto> Photos,
        string Message
    )> GetPhotosForModerationAsync()
    {
        _logger.LogDebug("AdminService - GetPhotosForModerationAsync invoked");

        var photos = await _unitOfWork.PhotoRepository.GetUnapprovedPhotos();
        if (photos == null || !photos.Any())
        {
            _logger.LogInformation("No photos need moderation at this time.");
            return (
                Enumerable.Empty<PhotoForApprovalDto>(),
                "No photos need moderation at this time."
            );
        }

        var photoDtos = photos.Select(p => _mapper.Map<PhotoForApprovalDto>(p));
        _logger.LogInformation("{Count} photos retrieved for moderation.", photoDtos.Count());
        return (photoDtos, "Photos retrieved successfully.");
    }

    public async Task<IEnumerable<object>> GetUsersWithRolesAsync()
    {
        _logger.LogDebug("AdminService - GetUsersWithRolesAsync invoked");

        var usersWithRoles = await _unitOfWork.UserRepository.GetUsersWithRolesAsync();
        _logger.LogInformation("{Count} users with roles retrieved.", usersWithRoles.Count());
        return usersWithRoles;
    }

    public async Task<IEnumerable<PhotoTagDto>> GetPhotoTagsAsync()
    {
        _logger.LogDebug("AdminService - GetPhotoTagsAsync invoked");

        var tags = await _unitOfWork.TagRepository.GetAllTagsAsync();
        if (tags == null || !tags.Any())
        {
            _logger.LogInformation("No photo tags found.");
            return Enumerable.Empty<PhotoTagDto>();
        }

        var tagDtos = tags.Select(t => _mapper.Map<PhotoTagDto>(t));
        _logger.LogInformation("{Count} photo tags retrieved.", tagDtos.Count());
        return tagDtos;
    }

    public async Task<PhotoTagDto> CreatePhotoTagAsync(string tagName)
    {
        if (string.IsNullOrWhiteSpace(tagName))
            throw new ArgumentException("Tag name cannot be empty.", nameof(tagName));

        _logger.LogDebug(
            "AdminService - CreatePhotoTagAsync invoked (tagName: {TagName})",
            tagName
        );

        var existing = await _unitOfWork.TagRepository.GetTagByNameAsync(tagName.Trim());
        if (existing != null)
            throw new InvalidOperationException("Tag already exists.");

        var tag = new Tag { Name = tagName.Trim() };
        await _unitOfWork.TagRepository.AddTagAsync(tag);
        await _unitOfWork.TagRepository.SaveAllAsync();

        _logger.LogInformation(
            "Tag '{TagName}' created successfully with ID {TagId}.",
            tagName,
            tag.Id
        );
        return _mapper.Map<PhotoTagDto>(tag);
    }

    public async Task<bool> DeletePhotoTagAsync(int tagId)
    {
        _logger.LogDebug("AdminService - DeletePhotoTagAsync invoked (tagId: {TagId})", tagId);

        var tag = await _unitOfWork.TagRepository.GetTagByIdAsync(tagId);
        if (tag == null)
        {
            _logger.LogWarning("Tag with ID {TagId} not found.", tagId);
            return false;
        }

        _unitOfWork.TagRepository.RemoveTag(tag);
        var success = await _unitOfWork.TagRepository.SaveAllAsync();

        if (success)
            _logger.LogInformation("Tag with ID {TagId} deleted successfully.", tagId);
        else
            _logger.LogWarning("Failed to delete tag with ID {TagId}.", tagId);

        return success;
    }

    public async Task<PhotoTagDto?> AddTagToPhotoAsync(int photoId, int tagId)
    {
        _logger.LogDebug(
            "AdminService - AddTagToPhotoAsync invoked (photoId: {PhotoId}, tagId: {TagId})",
            photoId,
            tagId
        );

        var photo = await _unitOfWork.PhotoRepository.GetPhotoById(photoId);
        if (photo == null)
            throw new KeyNotFoundException($"Photo with ID {photoId} not found.");

        var tag = await _unitOfWork.TagRepository.GetTagByIdAsync(tagId);
        if (tag == null)
            throw new KeyNotFoundException($"Tag with ID {tagId} not found.");

        if (photo.PhotoTags == null)
            photo.PhotoTags = new List<PhotoTag>();

        if (photo.PhotoTags.Any(pt => pt.TagId == tagId))
        {
            _logger.LogInformation(
                "Tag with ID {TagId} is already assigned to photo with ID {PhotoId}.",
                tagId,
                photoId
            );
            return _mapper.Map<PhotoTagDto>(tag);
        }

        photo.PhotoTags.Add(new PhotoTag { PhotoId = photoId, TagId = tagId });
        await _unitOfWork.Complete();

        _logger.LogInformation(
            "Tag with ID {TagId} added to photo with ID {PhotoId}.",
            tagId,
            photoId
        );
        return _mapper.Map<PhotoTagDto>(tag);
    }

    public async Task<bool> RemoveTagFromPhotoAsync(int photoId, int tagId)
    {
        _logger.LogDebug(
            "AdminService - RemoveTagFromPhotoAsync invoked (photoId: {PhotoId}, tagId: {TagId})",
            photoId,
            tagId
        );

        var photo = await _unitOfWork.PhotoRepository.GetPhotoById(photoId);
        if (photo == null || photo.PhotoTags == null)
        {
            _logger.LogWarning("Photo with ID {PhotoId} not found or has no tags.", photoId);
            return false;
        }

        var photoTag = photo.PhotoTags.FirstOrDefault(pt => pt.TagId == tagId);
        if (photoTag == null)
        {
            _logger.LogWarning(
                "Tag with ID {TagId} not found on photo with ID {PhotoId}.",
                tagId,
                photoId
            );
            return false;
        }

        photo.PhotoTags.Remove(photoTag);
        await _unitOfWork.Complete();

        _logger.LogInformation(
            "Tag with ID {TagId} removed from photo with ID {PhotoId}.",
            tagId,
            photoId
        );
        return true;
    }

    public async Task<IEnumerable<PhotoApprovalStatsDto>> GetPhotoApprovalStatsAsync()
    {
        var stats = await _context
            .PhotoApprovalStats.FromSqlRaw("EXEC GetPhotoApprovalStats")
            .ToListAsync();

        return stats.Select(p => new PhotoApprovalStatsDto
        {
            Username = p.Username,
            ApprovedPhotos = p.ApprovedPhotos,
            UnapprovedPhotos = p.UnapprovedPhotos,
        });
    }

    public async Task<IEnumerable<string>> GetUsersWithoutMainPhotoAsync()
    {
        var users = await _context
            .UsersWithoutMainPhoto.FromSqlRaw("EXEC GetUsersWithoutMainPhoto")
            .AsNoTracking()
            .ToListAsync();

        return users.Select(u => u.Username);
    }
}
