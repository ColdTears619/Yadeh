using Yadeh.Domain.Conversations.Enums;

namespace Yadeh.Domain.Conversations;

public sealed class ConversationSnapshot
{
    private readonly List<ConversationMessage> _messages = [];

    public Guid Id { get; private set; }

    public Guid ConversationId { get; private set; }

    public string? OriginalTitle { get; private set; }

    public string SourceUrl { get; private set; } = string.Empty;

    public DateTimeOffset ImportedAtUtc { get; private set; }

    public IReadOnlyCollection<ConversationMessage> Messages =>
        _messages.AsReadOnly();

    private ConversationSnapshot()
    {
    }

    internal ConversationSnapshot(
        Guid conversationId,
        string? originalTitle,
        Uri sourceUrl,
        DateTimeOffset importedAtUtc)
    {
        if (conversationId == Guid.Empty)
        {
            throw new ArgumentException(
                "The conversation identifier is required.",
                nameof(conversationId));
        }

        ArgumentNullException.ThrowIfNull(sourceUrl);

        if (!sourceUrl.IsAbsoluteUri)
        {
            throw new ArgumentException(
                "The source URL must be absolute.",
                nameof(sourceUrl));
        }

        if (importedAtUtc.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException(
                "The import time must use the UTC offset.",
                nameof(importedAtUtc));
        }

        Id = Guid.NewGuid();
        ConversationId = conversationId;
        OriginalTitle = NormalizeOptionalTitle(originalTitle);
        SourceUrl = sourceUrl.AbsoluteUri;
        ImportedAtUtc = importedAtUtc;
    }

    internal void AddMessage(
        int sequence,
        ConversationMessageRole role,
        string content)
    {
        if (_messages.Any(message => message.Sequence == sequence))
        {
            throw new InvalidOperationException(
                $"A message with sequence {sequence} already exists.");
        }

        _messages.Add(
            new ConversationMessage(
                Id,
                sequence,
                role,
                content));
    }

    private static string? NormalizeOptionalTitle(string? title)
    {
        return string.IsNullOrWhiteSpace(title)
            ? null
            : title.Trim();
    }
}