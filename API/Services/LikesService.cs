using System;
using API.DTOs;
using API.Helpers;
using API.Interfaces;

namespace API.Services;

public class LikesService : ILikesService
{

    private readonly IUnitOfWork _unitOfWork;

    public LikesService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    public async Task<IEnumerable<int>> GetCurrentUserLikeIdsAsync(int userId)
    {
         return await _unitOfWork.LikesRepository.GetCurrentUserLikeIds(userId);
    }

    public async Task<PagedList<MemberDto>> GetUserLikesAsync(LikesParams likesParams)
    {
       return await _unitOfWork.LikesRepository.GetUserLikes(likesParams);
    }

    public async Task<bool> ToggleLikeAsync(int sourceUserId, int targetUserId)
    {
         

        if (sourceUserId == targetUserId) throw new Exception ("You cannot like yourself");

        var existingLike = await _unitOfWork.LikesRepository.GetUserLike(sourceUserId, targetUserId);

        if (existingLike == null)
        {
            var like = new UserLike
            {
                SourceUserId = sourceUserId,
                TargetUserId = targetUserId
            };

            _unitOfWork.LikesRepository.AddLike(like);
        }
        else 
        {
            _unitOfWork.LikesRepository.DeleteLike(existingLike);
        }

        return await _unitOfWork.Complete();
    }
}
