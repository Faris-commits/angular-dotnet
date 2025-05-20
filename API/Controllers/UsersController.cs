using API.DTOs;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class UsersController(IUsersService usersService, ILogger<UsersController> logger) : BaseApiController
{
    private readonly IUsersService _usersService = usersService;
    private readonly ILogger<UsersController> _logger = logger;

    /// <summary>
    /// GET /api/users
    /// </summary>
    /// <param name="userParams"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(ActionResult<IEnumerable<MemberDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery] UserParams userParams)
    {
        try
        {
            _logger.LogDebug($"UsersController - {nameof(GetUsers)} invoked. (userParams: {userParams})");
            var currentUsername = User.GetUsername();
            var users = await _usersService.GetUsersAsync(userParams, currentUsername);
            Response.AddPaginationHeader(users);
            return Ok(users);
        }
        catch (Exception ex)
        {

            _logger.LogError(ex, "Exception in UsersController.GetUsers");
            throw;

        }
    }

    /// <summary>
    /// GET /api/users/{username}
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    [HttpGet("{username}")]
    [ProducesResponseType(typeof(ActionResult<MemberDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<ActionResult<MemberDto>> GetUser(string username)
    {
        try
        {
            _logger.LogDebug($"UsersController - {nameof(GetUser)} invoked. (username: {User})");
            var currentUsername = User.GetUsername();
            var user = await _usersService.GetUserAsync(username, currentUsername);
            return Ok(user);
        }
        catch (Exception ex)
        {

            _logger.LogError(ex, "Exception in UsersController.GetUser");
            throw;
        }

    }
    /// <summary>
    /// PUT /api/users
    /// </summary>
    /// <param name="memberUpdateDto"></param>
    /// <returns></returns>
    [HttpPut]
    [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
    {
        try
        {
            _logger.LogDebug($"UsersController - {nameof(UpdateUser)} invoked. (MemberUpdateDto: {memberUpdateDto})");
            await _usersService.UpdateUserAsync(User.GetUsername(), memberUpdateDto);
            return Ok();
        }
        catch (Exception ex)
        {

            _logger.LogError(ex, "Exception in UsersController.UpdateUser");
            throw;
        }
    }
    /// <summary>
    /// POST /api/users/add-photo
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    [HttpPost("add-photo")]
    [ProducesResponseType(typeof(ActionResult<PhotoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
    {
        try
        {
            _logger.LogDebug($"UsersController - {nameof(AddPhoto)} invoked. (IFormFile: {file})");
            var photo = await _usersService.AddPhotoAsync(User.GetUsername(), file);
            return Ok(photo);
        }
        catch (Exception ex)
        {

            _logger.LogError(ex, "Exception in UsersController.AddPhoto");
            throw;

        }
    }

    /// <summary>
    /// PUT /api/users/set-main-photo/{photoId:int}
    /// </summary>
    /// <param name="photoId"></param>
    /// <returns></returns>
    [HttpPut("set-main-photo/{photoId:int}")]
    [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
        try
        {
            _logger.LogDebug($"UsersController - {nameof(SetMainPhoto)} invoked. (photoId: {photoId})");
            await _usersService.SetMainPhotoAsync(User.GetUsername(), photoId);
            return NoContent();

        }
        catch (Exception ex)
        {

            _logger.LogError(ex,"Exception in UsersController.SetMainPhoto");
            throw;
        }
    }

    /// <summary>
    /// DELETE /api/users/delete-photo/{photoId:int}
    /// </summary>
    /// <param name="photoId"></param>
    /// <returns></returns>
    [HttpDelete("delete-photo/{photoId:int}")]
    [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
        try
        {
            _logger.LogDebug($"UsersController - {nameof(DeletePhoto)} invoked. (photoId: {photoId})");
            var success = await _usersService.DeletePhotoAsync(User.GetUsername(), photoId);
            return Ok();
        }
        catch (Exception ex)
        {

            _logger.LogError(ex, "Exception in UsersController.DeletePhoto");
            throw;
        }
    }
}


