using Yadeh.Domain.Conversations.Enums;

namespace Yadeh.Domain.Conversations;

public sealed class ConversationMessage
{
    public Guid Id { get; private set; }

    public Guid ConversationSnapshotId { get; private set; }

    public int Sequence { get; private set; }

    public ConversationMessageRole Role { get; private set; }

    public string Content { get; private set; } = string.Empty;

    private ConversationMessage()
    {
    }

    internal ConversationMessage(
        Guid conversationSnapshotId,
        int sequence,
        ConversationMessageRole role,
        string content)
    {
        if (conversationSnapshotId == Guid.Empty)
        {
            throw new ArgumentException(
                "The conversation snapshot identifier is required.",
                nameof(conversationSnapshotId));
        }

        ArgumentOutOfRangeException.ThrowIfNegative(sequence);

        if (!Enum.IsDefined(role))
        {
            throw new ArgumentOutOfRangeException(nameof(role));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        Id = Guid.NewGuid();
        ConversationSnapshotId = conversationSnapshotId;
        Sequence = sequence;
        Role = role;
        Content = content;
    }
}