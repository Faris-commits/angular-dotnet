using API.Controllers;
using API.DTOs;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API;

[Authorize]
public class MessagesController(IMessageService messageService) : BaseApiController
{
    [HttpPost]
    public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
    {
       try
       {
         var senderUsername = User.GetUsername();
         var result = await messageService.CreateMessageAsync(senderUsername, createMessageDto);
         if (result == null) return BadRequest("Could not send message");
         return Ok(result);
       }
       catch (Exception)
       {
        
        return StatusCode(500, "Server error while sending message");
       }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser(
        [FromQuery]MessageParams messageParams)
    {
        try
        {
            messageParams.Username = User.GetUsername();
    
            var messages = await messageService.GetMessagesForUserAsync(messageParams);
            Response.AddPaginationHeader(messages);
            return Ok(messages);
        
        }
        catch (Exception)
        {
            
           return StatusCode(500, "Server error while getting messages");
        }
    }

    [HttpGet("thread/{username}")]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
    {
        try
        {
            var currentUsername = User.GetUsername();
            var thread = await messageService.GetMessageThreadAsync(currentUsername, username);
            return Ok(thread);
        }
        catch (Exception)
        {
            
            return StatusCode(500, "Server error while getting message thread");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMessage(int id)
    {
        try
        {
          var  username = User.GetUsername();
            var success = await messageService.DeleteMessageAsync(username, id);
            if (!success) return BadRequest("Problem deleting message");
            return Ok();
        }
        catch (Exception)
        {
            
            return StatusCode(500, "Server error while deleting message");
        }
    }
}
