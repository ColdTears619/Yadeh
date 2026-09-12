using Yadeh.Domain.Conversations;
using Yadeh.Domain.Conversations.Enums;

namespace Yadeh.Domain.Tests.Conversations;

public sealed class ConversationSnapshotTests
{
    private static readonly DateTimeOffset CreatedAtUtc =
        new(2026, 9, 12, 8, 30, 0, TimeSpan.Zero);

    [Fact]
    public void AddSnapshot_WithValidValues_CreatesSnapshot()
    {
        var conversation = new Conversation("Title", CreatedAtUtc);
        var sourceUrl = new Uri(
            "https://chatgpt.com/share/conversation-id?locale=en");
        var importedAtUtc = CreatedAtUtc.AddMinutes(1);

        var snapshot = conversation.AddSnapshot(
            "  Original title  ",
            sourceUrl,
            importedAtUtc,
            [(0, ConversationMessageRole.User, "Message")]);

        Assert.NotEqual(Guid.Empty, snapshot.Id);
        Assert.Equal(conversation.Id, snapshot.ConversationId);
        Assert.Equal("Original title", snapshot.OriginalTitle);
        Assert.Equal(sourceUrl.AbsoluteUri, snapshot.SourceUrl);
        Assert.Equal(importedAtUtc, snapshot.ImportedAtUtc);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddSnapshot_WithoutOriginalTitle_StoresNull(
        string? originalTitle)
    {
        var conversation = new Conversation("Title", CreatedAtUtc);

        var snapshot = conversation.AddSnapshot(
            originalTitle,
            new Uri("https://chatgpt.com/share/conversation-id"),
            CreatedAtUtc.AddMinutes(1),
            [(0, ConversationMessageRole.User, "Message")]);

        Assert.Null(snapshot.OriginalTitle);
    }

    [Fact]
    public void AddSnapshot_WithRelativeSourceUrl_ThrowsException()
    {
        var conversation = new Conversation("Title", CreatedAtUtc);
        var relativeUrl = new Uri("share/conversation-id", UriKind.Relative);

        Assert.Throws<ArgumentException>(
            () => conversation.AddSnapshot(
                null,
                relativeUrl,
                CreatedAtUtc.AddMinutes(1),
                [(0, ConversationMessageRole.User, "Message")]));
        Assert.Empty(conversation.Snapshots);
    }

    [Fact]
    public void AddSnapshot_WithNonUtcImportTime_ThrowsException()
    {
        var conversation = new Conversation("Title", CreatedAtUtc);
        var nonUtcDate = new DateTimeOffset(
            2026,
            9,
            12,
            12,
            0,
            0,
            TimeSpan.FromHours(3.5));

        Assert.Throws<ArgumentException>(
            () => conversation.AddSnapshot(
                null,
                new Uri("https://chatgpt.com/share/conversation-id"),
                nonUtcDate,
                [(0, ConversationMessageRole.User, "Message")]));
        Assert.Empty(conversation.Snapshots);
    }

    [Fact]
    public void AddSnapshot_WithDuplicateMessageSequence_ThrowsException()
    {
        var conversation = new Conversation("Title", CreatedAtUtc);

        Assert.Throws<InvalidOperationException>(
            () => conversation.AddSnapshot(
                null,
                new Uri("https://chatgpt.com/share/conversation-id"),
                CreatedAtUtc.AddMinutes(1),
                [
                    (0, ConversationMessageRole.User, "First message"),
                    (0, ConversationMessageRole.Assistant, "Second message")
                ]));
        Assert.Empty(conversation.Snapshots);
    }
}
