using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class UsersController(
    IUsersService usersService,
    ILogger<UsersController> logger,
    IUnitOfWork unitOfWork,
    IMatchService matchService
) : BaseApiController
{
    private readonly IUsersService _usersService = usersService;
    private readonly ILogger<UsersController> _logger = logger;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMatchService _matchService = matchService;

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
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers(
        [FromQuery] UserParams userParams
    )
    {
        try
        {
            _logger.LogDebug(
                $"UsersController - {nameof(GetUsers)} invoked. (userParams: {userParams})"
            );
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
            _logger.LogDebug(
                $"UsersController - {nameof(UpdateUser)} invoked. (MemberUpdateDto: {memberUpdateDto})"
            );
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
            _logger.LogDebug(
                $"UsersController - {nameof(SetMainPhoto)} invoked. (photoId: {photoId})"
            );
            await _usersService.SetMainPhotoAsync(User.GetUsername(), photoId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in UsersController.SetMainPhoto");
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
            _logger.LogDebug(
                $"UsersController - {nameof(DeletePhoto)} invoked. (photoId: {photoId})"
            );
            var success = await _usersService.DeletePhotoAsync(User.GetUsername(), photoId);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in UsersController.DeletePhoto");
            throw;
        }
    }

    /// <summary>
    /// GET /api/users/photo-tags
    /// </summary>
    /// <returns></returns>
    [HttpGet("photo-tags")]
    [ProducesResponseType(typeof(IEnumerable<PhotoTagDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<PhotoTagDto>>> GetPhotoTags()
    {
        try
        {
            _logger.LogDebug("UsersController - GetPhotoTags invoked");
            var tags = await _unitOfWork.TagRepository.GetAllTagsAsync();
            var tagDtos = tags.Select(t => new PhotoTagDto { Id = t.Id, Name = t.Name });
            _logger.LogInformation("{Count} photo tags retrieved.", tagDtos.Count());
            return Ok(tagDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in UsersController.GetPhotoTags");
            return StatusCode(500, "An error occurred while retrieving photo tags.");
        }
    }

    /// <summary>
    /// POST /api/users/photo-tags
    /// </summary>
    /// <param name="dto">The tag creation DTO</param>
    /// <returns></returns>
    [HttpPost("photo-tags")]
    [ProducesResponseType(typeof(PhotoTagDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PhotoTagDto>> CreateTag([FromBody] CreateTagDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Tag name is required.");

            _logger.LogDebug("UsersController - CreateTag invoked (tagName: {TagName})", dto.Name);

            var existing = await _unitOfWork.TagRepository.GetTagByNameAsync(dto.Name.Trim());
            if (existing != null)
                return BadRequest("Tag already exists.");

            var tag = new Tag { Name = dto.Name.Trim() };
            await _unitOfWork.TagRepository.AddTagAsync(tag);
            await _unitOfWork.Complete();

            _logger.LogInformation(
                "Tag '{TagName}' created successfully with ID {TagId}.",
                dto.Name,
                tag.Id
            );
            return Ok(new PhotoTagDto { Id = tag.Id, Name = tag.Name });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in UsersController.CreateTag");
            return StatusCode(500, "An error occurred while creating the tag.");
        }
    }

    /// <summary>
    /// GET /api/users/photos/by-tag/{tagId}
    /// </summary>
    /// <param name="tagId">The ID of the tag</param>
    /// <returns></returns>
    [HttpGet("photos/by-tag/{tagId}")]
    [ProducesResponseType(typeof(IEnumerable<PhotoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<PhotoDto>>> GetPhotosByTag(int tagId)
    {
        try
        {
            _logger.LogDebug("UsersController - GetPhotosByTag invoked (tagId: {TagId})", tagId);

            var photos = await _usersService.GetPhotosByTagAsync(tagId);
            if (photos == null || !photos.Any())
            {
                _logger.LogWarning("No photos found for tag ID {TagId}.", tagId);
                return NotFound("No photos found for the specified tag.");
            }

            _logger.LogInformation(
                "{Count} photos retrieved for tag ID {TagId}.",
                photos.Count(),
                tagId
            );
            return Ok(photos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in UsersController.GetPhotosByTag");
            return StatusCode(500, "An error occurred while retrieving photos by tag.");
        }
    }

    /// <summary>
    /// POST /api/users/photos/{photoId}/tags
    /// </summary>
    /// <param name="photoId">The ID of the photo</param>
    /// <param name="tagIds">The list of tag IDs to assign</param>
    /// <returns></returns>
    [HttpPost("photos/{photoId}/tags")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> SetPhotoTags(int photoId, [FromBody] List<int> tagIds)
    {
        try
        {
            _logger.LogDebug(
                "UsersController - SetPhotoTags invoked (photoId: {PhotoId}, tagIds: {TagIds})",
                photoId,
                tagIds
            );

            var username = User.GetUsername();
            await _usersService.SetPhotoTagsAsync(username, photoId, tagIds);

            _logger.LogInformation(
                "Tags {TagIds} assigned to photo ID {PhotoId} by user {Username}.",
                tagIds,
                photoId,
                username
            );
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in UsersController.SetPhotoTags");
            return StatusCode(500, "An error occurred while assigning tags to the photo.");
        }
    }

    /// <summary>
    /// DELETE /api/users/tags/{id}
    /// </summary>
    /// <param name="id">The ID of the tag to delete</param>
    /// <returns></returns>
    [HttpDelete("tags/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteTag(int id)
    {
        try
        {
            _logger.LogDebug("UsersController - DeleteTag invoked (tagId: {TagId})", id);

            var result = await _usersService.DeleteTagAsync(id);
            if (!result)
            {
                _logger.LogWarning("Tag with ID {TagId} not found.", id);
                return NotFound();
            }

            _logger.LogInformation("Tag with ID {TagId} deleted successfully.", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in UsersController.DeleteTag");
            return StatusCode(500, "An error occurred while deleting the tag.");
        }
    }

    /// <summary>
    /// GET /api/users/matches
    /// </summary>
    /// <param name="gender"></param>
    /// <param name="city"></param>
    /// <returns></returns>
    [Authorize]
    [HttpGet("matches")]
    [ProducesResponseType(typeof(IEnumerable<MatchDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<MatchDto>>> GetMatches(
        [FromQuery] string? gender = null,
        [FromQuery] string? city = null
    )
    {
        try
        {
            _logger.LogDebug(
                $"UsersController - {nameof(GetMatches)} invoked. (gender: {gender}, city: {city})"
            );

            var userId = User.GetUserId();
            var matches = await _matchService.GetMatchesForUserAsync(userId, gender, city);

            if (matches == null || !matches.Any())
            {
                _logger.LogInformation("No matches found for user ID {UserId}.", userId);
                return Ok(new List<MatchDto>());
            }

            _logger.LogInformation(
                "{Count} matches retrieved for user ID {UserId}.",
                matches.Count(),
                userId
            );
            return Ok(matches);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in UsersController.GetMatches");
            return StatusCode(500, "An error occurred while retrieving matches.");
        }
    }

    /// <summary>
    /// GET /api/users/photos
    /// </summary>
    [HttpGet("photos")]
    [ProducesResponseType(typeof(IEnumerable<PhotoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<PhotoDto>>> GetAllPhotos()
    {
        try
        {
            _logger.LogDebug("UsersController - GetAllPhotos invoked");
            var photos = await _usersService.GetAllPhotosAsync();
            return Ok(photos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in UsersController.GetAllPhotos");
            return StatusCode(500, "An error occurred while retrieving photos.");
        }
    }
}
