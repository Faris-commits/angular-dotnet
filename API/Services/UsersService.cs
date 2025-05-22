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

        public async Task UpdateUserAsync(string username, MemberUpdateDto memberUpdateDto)
        {
            if (string.IsNullOrEmpty(username)) throw new ArgumentException("Username cannot be null or empty", nameof(username));
            if (memberUpdateDto == null) throw new ArgumentNullException(nameof(memberUpdateDto));

            var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);
            if (user == null) throw new KeyNotFoundException($"User with username {username} not found.");

            _mapper.Map(memberUpdateDto, user);

            if (!await _unitOfWork.Complete())
                throw new InvalidOperationException("Failed to update user");

            _logger.LogInformation("User with username {Username} updated successfully", username);
        }
    }
}
