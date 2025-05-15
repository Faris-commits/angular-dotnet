using API.DTOs;
using API.Helpers;
using API.Interfaces;
using AutoMapper;

namespace API.Services;

public class MessageService(IUnitOfWork unitOfWork, IMapper mapper) : IMessageService
{
    public async Task<MessageDto> CreateMessageAsync(string senderUsername, CreateMessageDto createMessageDto)
    {
         
        if (senderUsername == createMessageDto.RecipientUsername.ToLower())
            return null;
        
        var sender = await unitOfWork.UserRepository.GetUserByUsernameAsync(senderUsername);
        var recipient = await unitOfWork.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

        if (sender == null || recipient == null || sender.UserName == null || recipient.UserName == null) 
            return null;

        var message = new Message
        {
            Sender = sender,
            Recipient = recipient,
            SenderUsername = sender.UserName,
            RecipientUsername = recipient.UserName,
            Content = createMessageDto.Content
        };

        unitOfWork.MessageRepository.AddMessage(message);

        if (await unitOfWork.Complete()) return mapper.Map<MessageDto>(message);

        return null;
    }

    public async Task<bool> DeleteMessageAsync(string username, int messageId)
    {
        var message = await unitOfWork.MessageRepository.GetMessage(messageId);

     

        if (message == null) return false;

        if (message.SenderUsername != username && message.RecipientUsername != username) 
            return false;

        if (message.SenderUsername == username) message.SenderDeleted = true;
        if (message.RecipientUsername == username) message.RecipientDeleted = true;

        if (message.SenderDeleted && message.RecipientDeleted)
        unitOfWork.MessageRepository.DeleteMessage(message);

      return await unitOfWork.Complete();
    }

    public async Task<PagedList<MessageDto>> GetMessagesForUserAsync(MessageParams messageParams)
    {
         return await unitOfWork.MessageRepository.GetMessagesForUser(messageParams);
    }

    public async Task<IEnumerable<MessageDto>> GetMessageThreadAsync(string currentUsername, string recipientUsername)
    {
        return await unitOfWork.MessageRepository.GetMessageThread(currentUsername, recipientUsername);
    }
}
