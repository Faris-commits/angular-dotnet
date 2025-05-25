using API.Controllers;
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
            var photos = await adminService.GetPhotosForModerationAsync();

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
    /// <returns></returns>
    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpPost("reject-photo/{photoId}")]
    [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<ActionResult> RejectPhoto(int photoId)
    {
        try
        {
            _logger.LogDebug($"AdminController - {nameof(RejectPhoto)} invoked. (photoId: {photoId})");
            await adminService.RejectPhotoAsync(photoId);
            return Ok(photoId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in AdminController-RejectPhoto");
            throw;
        }
    }

    /// <summary>
/// GET /api/admin/photo-tags
/// </summary>
[Authorize(Policy = "RequireAdminRole")]
[HttpGet("photo-tags")]
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
        throw;
    }
}

    /// <summary>
    /// POST /api/admin/photo-tags
    /// </summary>
    [Authorize(Policy = "RequireAdminRole")]
    [HttpPost("photo-tags")]
    public async Task<ActionResult> CreatePhotoTag([FromBody] string tagName)
    {
        try
        {
            _logger.LogDebug("AdminController - CreatePhotoTag invoked (tagName: {tagName})", tagName);
            var tag = await adminService.CreatePhotoTagAsync(tagName);
            return Ok(tag);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in AdminController.CreatePhotoTag");
            throw;
        }
    }
/// <summary>
/// DELETE /api/admin/photo-tags/{tagId}
/// </summary>
[Authorize(Policy = "RequireAdminRole")]
[HttpDelete("photo-tags/{tagId}")]
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

}
