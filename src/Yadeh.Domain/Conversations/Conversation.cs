using Yadeh.Domain.Conversations.Enums;

namespace Yadeh.Domain.Conversations;

public sealed class Conversation
{
    private readonly List<ConversationSnapshot> _snapshots = [];

    public Guid Id { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public IReadOnlyCollection<ConversationSnapshot> Snapshots =>
        _snapshots.AsReadOnly();

    private Conversation()
    {
    }

    public Conversation(
        string title,
        DateTimeOffset createdAtUtc)
    {
        EnsureUtc(createdAtUtc, nameof(createdAtUtc));

        Id = Guid.NewGuid();
        Title = NormalizeRequiredTitle(title);
        CreatedAtUtc = createdAtUtc;
    }

    public void Rename(string title)
    {
        Title = NormalizeRequiredTitle(title);
    }

    public ConversationSnapshot AddSnapshot(
        string? originalTitle,
        Uri sourceUrl,
        DateTimeOffset importedAtUtc,
        IEnumerable<(
            int Sequence,
            ConversationMessageRole Role,
            string Content)> messages)
    {
        ArgumentNullException.ThrowIfNull(messages);

        var snapshot = new ConversationSnapshot(
            Id,
            originalTitle,
            sourceUrl,
            importedAtUtc);

        foreach (var message in messages)
        {
            snapshot.AddMessage(
                message.Sequence,
                message.Role,
                message.Content);
        }

        if (snapshot.Messages.Count == 0)
        {
            throw new ArgumentException(
                "A snapshot must contain at least one message.",
                nameof(messages));
        }

        _snapshots.Add(snapshot);

        return snapshot;
    }

    private static string NormalizeRequiredTitle(string title)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        return title.Trim();
    }

    private static void EnsureUtc(
        DateTimeOffset value,
        string parameterName)
    {
        if (value.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException(
                "The date and time must use the UTC offset.",
                parameterName);
        }
    }
}