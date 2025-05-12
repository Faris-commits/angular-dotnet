using System;
using API.DTOs;
using API.Helpers;

namespace API.Interfaces;

public interface ILikesService
{

    Task<bool> ToggleLikeAsync(int sourceUserId, int targetUserId);
    Task<IEnumerable<int>> GetCurrentUserLikeIdsAsync(int userId);

    Task<PagedList<MemberDto>> GetUserLikesAsync(LikesParams likesParams);

}
