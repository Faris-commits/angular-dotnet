using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;

namespace API.Services
{
    public class UsersService : IUsersService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;
        private readonly ILogger<UsersService> _logger;

        public UsersService(IUnitOfWork unitOfWork, IMapper mapper, IPhotoService photoService, ILogger<UsersService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _photoService = photoService;
            _logger = logger;
        }

        public async Task<PhotoDto> AddPhotoAsync(string username, IFormFile file)
        {
            if (string.IsNullOrEmpty(username)) throw new ArgumentException("Username cannot be null or empty", nameof(username));

            var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);
            if (user == null) throw new KeyNotFoundException($"User with username {username} not found.");

            var result = await _photoService.AddPhotoAsync(file);
            if (result.Error != null) throw new InvalidOperationException("Error uploading photo to Cloudinary");

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId,
                AppUserId = user.Id,
                IsApproved = false
            };

            user.Photos.Add(photo);

            if (!await _unitOfWork.Complete())
                throw new Exception("Problem adding photo to the database");

            _logger.LogInformation("Photo uploaded successfully for user {Username} with publicId {PublicId}", username, result.PublicId);

            return _mapper.Map<PhotoDto>(photo);
        }

        public async Task<bool> DeletePhotoAsync(string username, int photoId)
        {
            if (string.IsNullOrEmpty(username)) throw new ArgumentException("Username cannot be null or empty", nameof(username));
            if (photoId <= 0) throw new ArgumentException("Invalid photo ID", nameof(photoId));

            var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);
            if (user == null) throw new KeyNotFoundException($"User with username {username} not found.");

            var photo = await _unitOfWork.PhotoRepository.GetPhotoById(photoId);
            if (photo == null) throw new KeyNotFoundException($"Photo with ID {photoId} not found.");

            if (photo.IsMain) throw new InvalidOperationException("You can't delete your main photo");

            if (photo.PublicId != null)
            {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null) throw new InvalidOperationException("Error deleting photo from Cloudinary");
            }

            user.Photos.Remove(photo);

            if (!await _unitOfWork.Complete())
                throw new InvalidOperationException("Problem deleting photo from the database");

            _logger.LogInformation("Photo with ID {PhotoId} deleted successfully for user {Username}", photoId, username);

            return true;
        }

        public async Task<IEnumerable<PhotoDto>> GetPhotosByTagAsync(int tagId)
        {
            var photos = await _unitOfWork.PhotoRepository.GetPhotosByTagAsync(tagId);
            return photos.Select(p => _mapper.Map<PhotoDto>(p));
        }

        public async Task<MemberDto> GetUserAsync(string username, string currentUsername)
        {
            if (string.IsNullOrEmpty(username)) throw new ArgumentException("Username cannot be null or empty", nameof(username));
            return await _unitOfWork.UserRepository.GetMemberAsync(username, isCurrentUser: currentUsername == username);
        }

        public async Task<PagedList<MemberDto>> GetUsersAsync(UserParams userParams, string currentUsername)
        {
            if (userParams == null) throw new ArgumentNullException(nameof(userParams));

            userParams.CurrentUsername = currentUsername;
            return await _unitOfWork.UserRepository.GetMembersAsync(userParams);
        }

        public async Task<bool> SetMainPhotoAsync(string username, int photoId)
        {
            if (string.IsNullOrEmpty(username)) throw new ArgumentException("Username cannot be null or empty", nameof(username));
            if (photoId <= 0) throw new ArgumentException("Invalid photo ID", nameof(photoId));

            var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);
            if (user == null) return false;

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
            if (photo == null || photo.IsMain) return false;

            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
            if (currentMain != null) currentMain.IsMain = false;

            photo.IsMain = true;
            return await _unitOfWork.Complete();
        }

        public async Task SetPhotoTagsAsync(string username, int photoId, List<int> tagIds)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be null or empty.", nameof(username));

            if (tagIds == null || !tagIds.Any())
                throw new ArgumentException("Tag IDs cannot be null or empty.", nameof(tagIds));

            var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);
            if (user == null)
                throw new KeyNotFoundException($"User with username '{username}' not found.");

            var photo = await _unitOfWork.PhotoRepository.GetPhotoById(photoId);
            if (photo == null || photo.AppUserId != user.Id)
                throw new KeyNotFoundException($"Photo with ID {photoId} not found for user '{username}'.");

            if (photo.PhotoTags == null)
                photo.PhotoTags = new List<PhotoTag>();


            var existingTagIds = photo.PhotoTags.Select(pt => pt.TagId).ToList();
            var duplicateTags = tagIds.Intersect(existingTagIds).ToList();
            if (duplicateTags.Any())
                throw new InvalidOperationException("Duplicate tags detected. Tags must be unique.");

            foreach (var tagId in tagIds.Distinct())
            {
                if (!existingTagIds.Contains(tagId))
                {
                    var tag = await _unitOfWork.TagRepository.GetTagByIdAsync(tagId);
                    if (tag == null)
                        throw new KeyNotFoundException($"Tag with ID {tagId} not found.");

                    photo.PhotoTags.Add(new PhotoTag { PhotoId = photoId, TagId = tagId });
                }
            }

            var result = await _unitOfWork.Complete();
            if (!result)
                throw new InvalidOperationException("Failed to update photo tags.");
        }
        public async Task UpdateUserAsync(string username, MemberUpdateDto memberUpdateDto)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be null or empty.", nameof(username));

            if (memberUpdateDto == null)
                throw new ArgumentNullException(nameof(memberUpdateDto), "Member update data cannot be null.");

            _logger.LogDebug("UsersService - UpdateUserAsync invoked (username: {Username})", username);

            var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);
            if (user == null)
                throw new KeyNotFoundException($"User with username '{username}' not found.");

            _mapper.Map(memberUpdateDto, user);

            if (!await _unitOfWork.Complete())
                throw new InvalidOperationException("Failed to update user.");

            _logger.LogInformation("User with username {Username} updated successfully.", username);
        }

        public async Task<bool> DeleteTagAsync(int tagId)
        {
            if (tagId <= 0)
                throw new ArgumentException("Invalid tag ID.", nameof(tagId));

            _logger.LogDebug("UsersService - DeleteTagAsync invoked (tagId: {TagId})", tagId);

            var tag = await _unitOfWork.TagRepository.GetTagByIdAsync(tagId);
            if (tag == null)
            {
                _logger.LogWarning("Tag with ID {TagId} not found.", tagId);
                return false;
            }

            var photoTags = _unitOfWork.Context.PhotoTags.Where(pt => pt.TagId == tagId).ToList();
            if (photoTags.Any())
            {
                _logger.LogInformation("Removing {Count} photo tags associated with tag ID {TagId}.", photoTags.Count, tagId);
                _unitOfWork.Context.PhotoTags.RemoveRange(photoTags);
            }

            _unitOfWork.TagRepository.RemoveTag(tag);

            var result = await _unitOfWork.Complete();
            if (!result)
                throw new InvalidOperationException("Failed to delete tag.");

            _logger.LogInformation("Tag with ID {TagId} deleted successfully.", tagId);
            return true;
        }
    }
}