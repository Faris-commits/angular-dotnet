using API.Controllers;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API;

public class AdminController(
    UserManager<AppUser> userManager,
    IUnitOfWork unitOfWork,
    IPhotoService photoService,
    IAdminService adminService,
    ILogger<AdminController> _logger
    ) : BaseApiController
{

    /// <summary>
    /// GET /api/admin/users-with-roles
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "RequireAdminRole")]
    [HttpGet("users-with-roles")]
    [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<ActionResult> GetUsersWithRoles()
    {
        try
        {
            _logger.LogDebug($"AdminController - {nameof(GetUsersWithRoles)} invoked");
            var users = await adminService.GetUsersWithRolesAsync();

            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in AdminController.GetUserWithRoles");
            throw;
        }
    }
    /// <summary>
    /// POST /api/admin/edit-roles/{username}
    /// </summary>
    /// <param name="username"></param>
    /// <param name="roles"></param>
    /// <returns></returns>
    [Authorize(Policy = "RequireAdminRole")]
    [HttpPost("edit-roles/{username}")]
    [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<ActionResult> EditRoles(string username, string roles)
    {
        try
        {
            _logger.LogDebug($"AdminController - {nameof(EditRoles)} invoked (username: {username}, roles: {roles})");
            var updatedRoles = await adminService.EditRolesAsync(username, roles);
            return Ok(updatedRoles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in AdminController.EditRoles");
            throw;
        }
    }
    /// <summary>
    /// GET /api/admin/photos-to-moderate
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpGet("photos-to-moderate")]
    [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<ActionResult> GetPhotosForModeration()
    {
        try
        {
            _logger.LogDebug($"AdminController - {nameof(GetPhotosForModeration)} invoked");
            var (photos, _) = await adminService.GetPhotosForModerationAsync();

            return Ok(photos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in AdminController.GetPhotosForModeration");
            throw;
        }
    }
    /// <summary>
    /// POST /api/approve-photo/{photoId}
    /// </summary>
    /// <param name="photoId"></param>
    /// <returns></returns>
    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpPost("approve-photo/{photoId}")]
    [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<ActionResult> ApprovePhoto(int photoId)
    {
        try
        {
            _logger.LogDebug($"AdminController - {nameof(ApprovePhoto)} invoked. (photoId: {photoId})");
            await adminService.ApprovePhotoAsync(photoId);
            return Ok(photoId);
        }
        catch (Exception ex)
        {

            _logger.LogError(ex, "Exception in AdminController.ApprovePhoto");
            throw;
        }
    }
/// <summary>
/// POST /api/admin/reject-photo/{photoId}
/// </summary>
/// <param name="photoId"></param>
/// <param name="dto"></param>
/// <returns></returns>
[Authorize(Policy = "ModeratePhotoRole")]
[HttpPost("reject-photo/{photoId}")]
public async Task<ActionResult> RejectPhoto(int photoId, [FromBody] PhotoRejectionDto dto)
{
    try
    {
        _logger.LogDebug($"AdminController - {nameof(RejectPhoto)} invoked. (photoId: {photoId})");
        await adminService.RejectPhotoAsync(photoId, dto.Reason);
        return Ok(photoId);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Exception in AdminController-RejectPhoto");
        return StatusCode(500, "An error occurred while rejecting the photo .");
    }
}

    /// <summary>
    /// GET /api/admin/photo-tags
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "RequireAdminRole")]
    [HttpGet("photo-tags")]
    [ProducesResponseType(typeof(IEnumerable<PhotoTagDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> GetPhotoTags()
    {
        try
        {
            _logger.LogDebug("AdminController - GetPhotoTags invoked");
            var tags = await adminService.GetPhotoTagsAsync();
            return Ok(tags);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in AdminController.GetPhotoTags");
            return StatusCode(500, "An error occurred while retrieving photo tags.");
        }
    }

    /// <summary>
    /// POST /api/admin/photo-tags
    /// </summary>
    /// <param name="dto">The tag creation DTO</param>
    /// <returns></returns>
    [Authorize(Policy = "RequireAdminRole")]
    [HttpPost("photo-tags")]
    [ProducesResponseType(typeof(PhotoTagDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> CreatePhotoTag([FromBody] CreateTagDto dto)
    {
        try
        {
            _logger.LogDebug("AdminController - CreatePhotoTag invoked (tagName: {tagName})", dto.Name);
            var tag = await adminService.CreatePhotoTagAsync(dto.Name);
            return Ok(tag);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in AdminController.CreatePhotoTag");
            return StatusCode(500, "An error occurred while creating the tag.");
        }
    }

    /// <summary>
    /// DELETE /api/admin/photo-tags/{tagId}
    /// </summary>
    /// <param name="tagId">The ID of the tag to delete</param>
    /// <returns></returns>
    [Authorize(Policy = "RequireAdminRole")]
    [HttpDelete("photo-tags/{tagId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeletePhotoTag(int tagId)
    {
        try
        {
            _logger.LogDebug("AdminController - DeletePhotoTag invoked (tagId: {tagId})", tagId);
            var result = await adminService.DeletePhotoTagAsync(tagId);
            if (!result) return NotFound();
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in AdminController.DeletePhotoTag");
            return StatusCode(500, "An error occurred while deleting the tag.");
        }
    }

    /// <summary>
    /// POST /api/admin/photos/{photoId}/tags/{tagId}
    /// </summary>
    /// <param name="photoId">The ID of the photo</param>
    /// <param name="tagId">The ID of the tag</param>
    /// <returns></returns>
    [Authorize(Policy = "RequireAdminRole")]
    [HttpPost("photos/{photoId}/tags/{tagId}")]
    [ProducesResponseType(typeof(PhotoTagDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> AddTagToPhoto(int photoId, int tagId)
    {
        try
        {
            _logger.LogDebug("AdminController - AddTagToPhoto invoked (photoId: {photoId}, tagId: {tagId})", photoId, tagId);
            var tag = await adminService.AddTagToPhotoAsync(photoId, tagId);
            if (tag == null) return BadRequest("Could not add tag to photo.");
            return Ok(tag);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in AdminController.AddTagToPhoto");
            return StatusCode(500, "An error occurred while adding the tag to the photo.");
        }
    }

    /// <summary>
    /// DELETE /api/admin/photos/{photoId}/tags/{tagId}
    /// </summary>
    /// <param name="photoId">The ID of the photo</param>
    /// <param name="tagId">The ID of the tag</param>
    /// <returns></returns>
    [Authorize(Policy = "RequireAdminRole")]
    [HttpDelete("photos/{photoId}/tags/{tagId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> RemoveTagFromPhoto(int photoId, int tagId)
    {
        try
        {
            _logger.LogDebug("AdminController - RemoveTagFromPhoto invoked (photoId: {photoId}, tagId: {tagId})", photoId, tagId);
            var result = await adminService.RemoveTagFromPhotoAsync(photoId, tagId);
            if (!result) return NotFound();
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in AdminController.RemoveTagFromPhoto");
            return StatusCode(500, "An error occurred while removing the tag from the photo.");
        }
    }

    /// <summary>
    /// GET /api/admin/photo-approval-stats
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "RequireAdminRole")]
    [HttpGet("photo-approval-stats")]
    [ProducesResponseType(typeof(IEnumerable<PhotoApprovalStatsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<PhotoApprovalStatsDto>>> GetPhotoApprovalStats()
    {
        try
        {
            _logger.LogDebug("AdminController - GetPhotoApprovalStats invoked");
            var stats = await adminService.GetPhotoApprovalStatsAsync();
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in AdminController.GetPhotoApprovalStats");
            return StatusCode(500, "An error occurred while retrieving photo approval statistics.");
        }
    }

    /// <summary>
    /// GET /api/admin/users-without-main-photo
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "RequireAdminRole")]
    [HttpGet("users-without-main-photo")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<string>>> GetUsersWithoutMainPhoto()
    {
        try
        {
            _logger.LogDebug("AdminController - GetUsersWithoutMainPhoto invoked");
            var users = await adminService.GetUsersWithoutMainPhotoAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in AdminController.GetUsersWithoutMainPhoto");
            return StatusCode(500, "An error occurred while retrieving users without main photo.");
        }
    }

}
