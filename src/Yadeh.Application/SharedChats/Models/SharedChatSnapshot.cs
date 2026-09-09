namespace Yadeh.Application.SharedChats.Models;

public sealed class SharedChatSnapshot
{
    public string? Title { get; }

    public IReadOnlyList<SharedChatMessage> Messages { get; }

    public SharedChatSnapshot(
        string? title,
        IEnumerable<SharedChatMessage> messages)
    {
        ArgumentNullException.ThrowIfNull(messages);

        var messageList = messages.ToArray();

        if (messageList.Length == 0)
        {
            throw new ArgumentException(
                "A shared chat snapshot must contain at least one message.",
                nameof(messages));
        }

        Title = string.IsNullOrWhiteSpace(title)
            ? null
            : title.Trim();

        Messages = Array.AsReadOnly(messageList);
    }
}