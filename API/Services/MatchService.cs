using API;
using API.DTOs;

public class MatchService : IMatchService
{
    private readonly IUserRepository _userRepository;

    public MatchService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<MatchDto>> GetMatchesForUserAsync(int userId)
    {
        return await _userRepository.GetMatchesForUserAsync(userId);
    }
}
