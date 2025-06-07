using API.DTOs;

public interface IMatchService
{
    Task<IEnumerable<MatchDto>> GetMatchesForUserAsync(int userId);
}
