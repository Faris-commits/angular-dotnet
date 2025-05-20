using System;
using API.DTOs;
using API.Helpers;

namespace API.Interfaces;

public interface IMessageService
{

    Task<MessageDto> CreateMessageAsync(string senderUsername, CreateMessageDto createMessageDto);
    Task<PagedList<MessageDto>> GetMessagesForUserAsync(MessageParams messageParams);
    Task<IEnumerable<MessageDto>> GetMessageThreadAsync(string currentUsername, string recipientUsername);
    Task<bool> DeleteMessageAsync(string username, int messageId);


}
