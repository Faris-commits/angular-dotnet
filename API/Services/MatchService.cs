using API;
using API.DTOs;
using Microsoft.Extensions.Logging;

public class MatchService : IMatchService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<MatchService> _logger;

    public MatchService(IUserRepository userRepository, ILogger<MatchService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<MatchDto>> GetMatchesForUserAsync(
        int userId,
        string? gender = null,
        string? city = null
    )
    {
        try
        {
            _logger.LogDebug(
                $"MatchService - {nameof(GetMatchesForUserAsync)} invoked. (userId: {userId}, gender: {gender}, city: {city})"
            );

            var matches = await _userRepository.GetMatchesForUserAsync(userId, gender, city);

            if (matches == null || !matches.Any())
            {
                _logger.LogInformation("No matches found for user ID {UserId}.", userId);
                return new List<MatchDto>();
            }

            _logger.LogInformation(
                "{Count} matches retrieved for user ID {UserId}.",
                matches.Count(),
                userId
            );
            return matches;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in MatchService.GetMatchesForUserAsync");
            throw;
        }
    }
}
