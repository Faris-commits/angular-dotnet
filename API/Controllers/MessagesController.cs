using API.DTOs;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class MessagesController(IMessageService messageService, ILogger<MessagesController> _logger)
    : BaseApiController
{
    /// <summary>
    /// POST /api/messages
    /// </summary>
    /// <param name="createMessageDto"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
    {
        try
        {
            _logger.LogDebug(
                $"MessagesController - {nameof(CreateMessage)} invoked. (createMessageDto: {createMessageDto})"
            );
            var username = User.GetUsername();
            var message = await messageService.CreateMessageAsync(username, createMessageDto);
            return Ok(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in MessagesController.CreateMessage");
            throw;
        }
    }

    /// <summary>
    /// GET /api/messages?container={messageParams}
    /// </summary>
    /// <param name="messageParams"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(ActionResult<IEnumerable<MessageDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser(
        [FromQuery] MessageParams messageParams
    )
    {
        try
        {
            _logger.LogDebug(
                $"MessagesController - {nameof(GetMessagesForUser)} invoked. (messageParams: {messageParams})"
            );
            messageParams.Username = User.GetUsername();
            var messages = await messageService.GetMessagesForUserAsync(messageParams);
            Response.AddPaginationHeader(messages);
            return Ok(messages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in MessagesController.GetMessagesForUser");
            throw;
        }
    }

    /// <summary>
    /// GET /api/messages/thread/{username}
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    [HttpGet("thread/{username}")]
    [ProducesResponseType(typeof(ActionResult<IEnumerable<MessageDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
    {
        try
        {
            _logger.LogDebug(
                $"MessagesController - {nameof(GetMessageThread)} invoked. (username: {username})"
            );
            var currentUsername = User.GetUsername();
            var thread = await messageService.GetMessageThreadAsync(currentUsername, username);
            return Ok(thread);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in MessagesController.GetMessageThread");
            throw;
        }
    }

    /// <summary>
    /// DELETE /api/messages/{id}
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<ActionResult> DeleteMessage(int id)
    {
        try
        {
            _logger.LogDebug($"MessagesController - {nameof(DeleteMessage)} invoked. (id : {id})");
            var username = User.GetUsername();
            await messageService.DeleteMessageAsync(username, id);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in MessagesController.DeleteMessage");
            throw;
        }
    }
}
