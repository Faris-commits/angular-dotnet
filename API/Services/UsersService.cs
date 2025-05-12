using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;

namespace API.Services;

public class UsersService : IUsersService
{
private readonly IUnitOfWork _unitOfWork;
private readonly IMapper _mapper;

private readonly IPhotoService _photoService;

public UsersService(IUnitOfWork unitOfWork, IMapper mapper, IPhotoService photoService)
{
    _unitOfWork = unitOfWork;
    _mapper = mapper;
    _photoService = photoService;
}

    
    public async Task<PhotoDto> AddPhotoAsync(string username, IFormFile file)
    { 
        var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);

        if (user == null) return null;

        var result = await _photoService.AddPhotoAsync(file);
        
        if (result.Error != null) throw new Exception(result.Error.Message);

        var photo = new Photo
        {
            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId
        };

      

        user.Photos.Add(photo);

        if (await _unitOfWork.Complete()) 
            return  _mapper.Map<PhotoDto>(photo);

        throw new Exception ("Problem adding photo");
    }

    public async Task<bool> DeletePhotoAsync(string username, int photoId)
    {
         var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);

        if (user == null) return false;

        var photo = await _unitOfWork.PhotoRepository.GetPhotoById(photoId);

        if (photo == null || photo.IsMain) return false;

        if (photo.PublicId != null)
        {
            var result = await _photoService.DeletePhotoAsync(photo.PublicId);
            if (result.Error != null) throw new Exception (result.Error.Message);
        }

        user.Photos.Remove(photo);

        return await _unitOfWork.Complete();
    }

    public async Task<MemberDto> GetUserAsync(string username, string currentUsername)
    {
        
        return await _unitOfWork.UserRepository.GetMemberAsync(username, isCurrentUser: currentUsername == username);
    }

    public async  Task<PagedList<MemberDto>> GetUsersAsync(UserParams userParams, string currentUsername)
    {
        userParams.CurrentUsername = currentUsername;
        return await _unitOfWork.UserRepository.GetMembersAsync(userParams);

    }

    public async Task<bool> SetMainPhotoAsync(string username, int photoId)
    {
        var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);

        if (user == null) return false;

        var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

        if (photo == null || photo.IsMain) return false;

        var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
        if (currentMain != null) currentMain.IsMain = false;
        photo.IsMain = true;

        return await _unitOfWork.Complete();

    }

    public async Task<bool> UpdateUserAsync(string currentUsername, MemberUpdateDto dto)
    {
         var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(currentUsername);

        if (user == null) return false;

        _mapper.Map(dto, user);

        return await _unitOfWork.Complete();
    }
}
