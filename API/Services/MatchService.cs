using API;
using API.DTOs;

public class MatchService : IMatchService
{
    private readonly IUserRepository _userRepository;

    public MatchService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<MatchDto>> GetMatchesForUserAsync(
        int userId,
        string? gender = null,
        string? city = null
    )
    {
        return await _userRepository.GetMatchesForUserAsync(userId, gender, city);
    }
}
