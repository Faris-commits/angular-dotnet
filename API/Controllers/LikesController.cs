using API.DTOs;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API;

[ApiController]
[Route("api/[controller]")]
public class LikesController : ControllerBase
{

    private readonly ILikesService _likesService;
    private readonly ILogger<LikesController> _logger;

    public LikesController(ILikesService likesService, ILogger<LikesController> logger)
    {
        _likesService = likesService;
        _logger = logger;

    }

    /// <summary>
    /// POST /api/likes/{targetUserId:int}
    /// </summary>
    /// <param name="targetUserId"></param>
    /// <returns></returns>
    [HttpPost("{targetUserId:int}")]
    [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<ActionResult> ToggleLike(int targetUserId)
    {
        try
        {
            _logger.LogDebug($"LikesController - {nameof(ToggleLike)} invoke. (targetUserId: {targetUserId})");
            var sourceUserId = User.GetUserId();
            await _likesService.ToggleLikeAsync(sourceUserId, targetUserId);
            return Ok();
        }
        catch (Exception ex)
        {

            _logger.LogError(ex, "Exception in LikesController.ToggleLike");
            throw;
        }

    }

    /// <summary>
    /// GET /api/likes/list
    /// </summary>
    /// <returns></returns>
    [HttpGet("list")]
    [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<ActionResult<IEnumerable<int>>> GetCurrentUserLikeIds()
    {
        try
        {
            _logger.LogDebug($"LikesController - {nameof(GetCurrentUserLikeIds)} invoked");
            var userId = User.GetUserId();
            var ids = await _likesService.GetCurrentUserLikeIdsAsync(userId);
            return Ok(ids);
        }
        catch (Exception ex)
        {

            _logger.LogError(ex, "Exception in GetCurrentUserLikeIds");
            throw;
        }



    }
    /// <summary>
    /// GET  /api/likes?predicate={likesParams}
    /// </summary>
    /// <param name="likesParams"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUserLikes([FromQuery] LikesParams likesParams)
    {
        try
        {
            _logger.LogDebug($"LikesController - {nameof(GetUserLikes)} invoke. (likesParams: {likesParams})");
            likesParams.UserId = User.GetUserId();
            var users = await _likesService.GetUserLikesAsync(likesParams);
            Response.AddPaginationHeader(users);
            return Ok(users);
        }
        catch (Exception ex)
        {

            _logger.LogError(ex, "Exception in LikesController.GetUserLikes");
            throw;
        }
    }
}
