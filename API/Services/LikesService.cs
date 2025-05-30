using API.DTOs;
using API.Helpers;
using API.Interfaces;
using ILogger = Serilog.ILogger;

namespace API.Services;

public class LikesService : ILikesService
{

    private readonly IUnitOfWork _unitOfWork;

    private readonly ILogger _logger;

    public LikesService(IUnitOfWork unitOfWork, ILogger logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
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
        if (sourceUserId <= 0 || targetUserId <= 0)
        {
            throw new ArgumentException("User IDs must be positive integers");
        }

        if (sourceUserId == targetUserId) throw new InvalidOperationException("You cannot like yourself");

        var existingLike = await _unitOfWork.LikesRepository.GetUserLike(sourceUserId, targetUserId);

        if (existingLike == null)
        {
            var like = new UserLike
            {
                SourceUserId = sourceUserId,
                TargetUserId = targetUserId
            };

            _unitOfWork.LikesRepository.AddLike(like);
            _logger.Information("User {SourceUserId} liked User {TargetUserId}", sourceUserId, targetUserId);
        }
        else
        {
            _unitOfWork.LikesRepository.DeleteLike(existingLike);
            _logger.Information("User {SourceUserId} unliked User {TargetUserId}", sourceUserId, targetUserId);
        }
        var result = await _unitOfWork.Complete();
        if (!result)
        {
            _logger.Error("Failed to update like status between SourceUserId: {SourceUserId} and TargetUserId: {TargetUserId}", sourceUserId, targetUserId);
        }
        return result;
    }
}
