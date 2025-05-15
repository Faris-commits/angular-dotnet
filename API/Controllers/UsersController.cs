using API.DTOs;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class UsersController(IUsersService usersService) : BaseApiController
{
private readonly IUsersService _usersService  = usersService;


    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery]UserParams userParams)
    {
       try
       {
         var currentUsername = User.GetUsername();
         var users = await _usersService.GetUsersAsync(userParams, currentUsername);
         Response.AddPaginationHeader(users);
         return Ok(users);
       }
       catch (Exception ex)
       {
        
        return StatusCode(500, $"Error getting users: {ex.Message}");

       }
    }

    [HttpGet("{username}")]  // /api/users/2
    public async Task<ActionResult<MemberDto>> GetUser(string username)
    {
        try
        {
            var currentUsername = User.GetUsername();
            var user = await _usersService.GetUserAsync(username, currentUsername);
            return Ok(user);
        }
        catch (Exception ex)
        {
            
            return StatusCode(500, $"Error getting user: {ex.Message}");
        }

    }

    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
    {
        try
        {
            var success = await _usersService.UpdateUserAsync(User.GetUsername(), memberUpdateDto);
            return success ? NoContent() : BadRequest("Failed to update the user");
        }
        catch (Exception ex)
        {
            
            return StatusCode(500, $"Error updating user: {ex.Message}");
        }
    }

    [HttpPost("add-photo")]
    public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
    {
       try
       {
         var photo = await _usersService.AddPhotoAsync(User.GetUsername(), file);
         if(photo == null) return BadRequest("Cant update user");
         return CreatedAtAction(nameof(GetUser), new { username = User.GetUsername()}, photo);
       }
       catch (Exception ex)
       {
        
       return BadRequest(ex.Message);

       }
    }

    [HttpPut("set-main-photo/{photoId:int}")]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
     try
     {
           var success = await _usersService.SetMainPhotoAsync(User.GetUsername(), photoId);
           return success ? NoContent() : BadRequest("Problem setting main photo");
     }
     catch 
     {
        
        return StatusCode(500, $"Error setting main photo");
     }
    }

    [HttpDelete("delete-photo/{photoId:int}")]
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
       try
       {
         var success = await _usersService.DeletePhotoAsync(User.GetUsername(), photoId);
         return success ? Ok() : BadRequest ("Problem deleting photo");
       }
       catch (Exception ex)
       {
        
        return BadRequest(ex.Message);
       }
    }
}


