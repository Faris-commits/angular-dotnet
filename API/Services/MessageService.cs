using API.DTOs;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Serilog;
using ILogger = Serilog.ILogger;

namespace API.Services;

public class MessageService(IUnitOfWork unitOfWork, IMapper mapper, ILogger logger)
    : IMessageService
{
    public async Task<MessageDto> CreateMessageAsync(
        string senderUsername,
        CreateMessageDto createMessageDto
    )
    {
        if (string.IsNullOrWhiteSpace(senderUsername))
            throw new ArgumentException(
                "Sender username must be provided.",
                nameof(senderUsername)
            );

        if (createMessageDto == null)
            throw new ArgumentNullException(nameof(createMessageDto));

        if (string.IsNullOrWhiteSpace(createMessageDto.RecipientUsername))
            throw new ArgumentException(
                "Recipient username must be provided.",
                nameof(createMessageDto.RecipientUsername)
            );

        if (string.IsNullOrWhiteSpace(createMessageDto.Content))
            throw new ArgumentException(
                "Message content cannot be empty.",
                nameof(createMessageDto.Content)
            );

        if (
            string.Equals(
                senderUsername,
                createMessageDto.RecipientUsername,
                StringComparison.OrdinalIgnoreCase
            )
        )
            throw new InvalidOperationException("You cannot send a message to yourself.");

        var sender = await unitOfWork.UserRepository.GetUserByUsernameAsync(senderUsername);
        if (sender?.UserName == null)
            throw new ArgumentException(
                "Sender not found or missing username.",
                nameof(senderUsername)
            );

        var recipient = await unitOfWork.UserRepository.GetUserByUsernameAsync(
            createMessageDto.RecipientUsername
        );
        if (recipient?.UserName == null)
            throw new ArgumentException(
                "Recipient not found or missing username.",
                nameof(createMessageDto.RecipientUsername)
            );

        var message = new Message
        {
            Sender = sender,
            Recipient = recipient,
            SenderUsername = sender.UserName,
            RecipientUsername = recipient.UserName,
            Content = createMessageDto.Content,
        };

        unitOfWork.MessageRepository.AddMessage(message);

        var success = await unitOfWork.Complete();
        if (!success)
            throw new Exception("Failed to save the message.");

        return mapper.Map<MessageDto>(message);
    }

    public async Task<bool> DeleteMessageAsync(string username, int messageId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username must be provided.", nameof(username));

            if (messageId <= 0)
                throw new ArgumentException("Invalid message ID.", nameof(messageId));

            var message = await unitOfWork.MessageRepository.GetMessage(messageId);
            if (message == null)
                throw new Exception("Message not found.");

            var isSender = string.Equals(
                message.SenderUsername,
                username,
                StringComparison.OrdinalIgnoreCase
            );
            var isRecipient = string.Equals(
                message.RecipientUsername,
                username,
                StringComparison.OrdinalIgnoreCase
            );

            if (!isSender && !isRecipient)
                throw new UnauthorizedAccessException("You are not authorized to delete this");

            if (isSender)
                message.SenderDeleted = true;
            if (isRecipient)
                message.RecipientDeleted = true;

            if (message.SenderDeleted && message.RecipientDeleted)
                unitOfWork.MessageRepository.DeleteMessage(message);

            var success = await unitOfWork.Complete();
            if (!success)
                throw new Exception("Unable to delete message");

            return true;
        }
        catch (Exception ex)
        {
            Log.Error(
                ex,
                "Error deleting message {MessageId} by user {Username}",
                messageId,
                username
            );
            throw;
        }
    }

    public async Task<PagedList<MessageDto>> GetMessagesForUserAsync(MessageParams messageParams)
    {
        return await unitOfWork.MessageRepository.GetMessagesForUser(messageParams);
    }

    public async Task<IEnumerable<MessageDto>> GetMessageThreadAsync(
        string currentUsername,
        string recipientUsername
    )
    {
        return await unitOfWork.MessageRepository.GetMessageThread(
            currentUsername,
            recipientUsername
        );
    }
}
