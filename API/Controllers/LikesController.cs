using API.Controllers;
using API.DTOs;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API;

public class LikesController : ControllerBase
{

    private readonly ILikesService _likesService;

    public LikesController(ILikesService likesService)
    {
        _likesService = likesService;
    }

    [HttpPost("{targetUserId:int}")]
    public async Task<ActionResult> ToggleLike(int targetUserId)
    {
       try
       {
         var sourceUserId = User.GetUserId();
        var success = await _likesService.ToggleLikeAsync(sourceUserId, targetUserId);
        if(success) return Ok();
        return BadRequest("Failed to toggle like");
       }
       catch (Exception ex)
       {
        
        return BadRequest(ex.Message);
       }
       
    }

    [HttpGet("list")]
    public async Task<ActionResult<IEnumerable<int>>> GetCurrentUserLikeIds()
    {
       try
       {
         var userId = User.GetUserId();
         var ids = await _likesService.GetCurrentUserLikeIdsAsync(userId);
         return Ok(ids);
       }
       catch (Exception)
       {
        
        return StatusCode(500, "Server error while getting ids");
       }
       
  
      
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUserLikes([FromQuery]LikesParams likesParams)
    {
        try
        {
            likesParams.UserId = User.GetUserId();
            var users = await _likesService.GetUserLikesAsync(likesParams);
            Response.AddPaginationHeader(users);
            return Ok(users);
        }
        catch (Exception)
        {
            
           return StatusCode(500, "Server error while getting likes");
        }
    }
}
