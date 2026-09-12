using Yadeh.Domain.Conversations;
using Yadeh.Domain.Conversations.Enums;

namespace Yadeh.Domain.Tests.Conversations;

public sealed class ConversationTests
{
    private static readonly DateTimeOffset CreatedAtUtc =
        new(2026, 9, 12, 8, 30, 0, TimeSpan.Zero);

    private static readonly Uri SourceUrl =
        new("https://chatgpt.com/share/conversation-id");

    [Fact]
    public void Constructor_WithValidValues_CreatesConversation()
    {
        var conversation = new Conversation(
            "  Architecture discussion  ",
            CreatedAtUtc);

        Assert.NotEqual(Guid.Empty, conversation.Id);
        Assert.Equal("Architecture discussion", conversation.Title);
        Assert.Equal(CreatedAtUtc, conversation.CreatedAtUtc);
        Assert.Empty(conversation.Snapshots);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidTitle_ThrowsException(
        string? title)
    {
        Assert.ThrowsAny<ArgumentException>(
            () => new Conversation(title!, CreatedAtUtc));
    }

    [Fact]
    public void Constructor_WithNonUtcDate_ThrowsException()
    {
        var nonUtcDate = new DateTimeOffset(
            2026,
            9,
            12,
            12,
            0,
            0,
            TimeSpan.FromHours(3.5));

        Assert.Throws<ArgumentException>(
            () => new Conversation("Title", nonUtcDate));
    }

    [Fact]
    public void Rename_WithValidTitle_ChangesAndNormalizesTitle()
    {
        var conversation = new Conversation("Old title", CreatedAtUtc);

        conversation.Rename("  New title  ");

        Assert.Equal("New title", conversation.Title);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Rename_WithInvalidTitle_ThrowsAndKeepsCurrentTitle(
        string? title)
    {
        var conversation = new Conversation("Current title", CreatedAtUtc);

        Assert.ThrowsAny<ArgumentException>(
            () => conversation.Rename(title!));
        Assert.Equal("Current title", conversation.Title);
    }

    [Fact]
    public void AddSnapshot_WithMessages_AttachesSnapshotToConversation()
    {
        var conversation = new Conversation("Title", CreatedAtUtc);

        var snapshot = conversation.AddSnapshot(
            "Original title",
            SourceUrl,
            CreatedAtUtc.AddMinutes(1),
            [
                (0, ConversationMessageRole.User, "Question"),
                (1, ConversationMessageRole.Assistant, "Answer")
            ]);

        Assert.Same(snapshot, Assert.Single(conversation.Snapshots));
        Assert.Equal(conversation.Id, snapshot.ConversationId);
        Assert.Equal(2, snapshot.Messages.Count);
    }

    [Fact]
    public void AddSnapshot_WithSameSourceUrlTwice_KeepsBothVersions()
    {
        var conversation = new Conversation("Title", CreatedAtUtc);

        conversation.AddSnapshot(
            "Version one",
            SourceUrl,
            CreatedAtUtc.AddMinutes(1),
            [(0, ConversationMessageRole.User, "First message")]);

        conversation.AddSnapshot(
            "Version two",
            SourceUrl,
            CreatedAtUtc.AddMinutes(2),
            [
                (0, ConversationMessageRole.User, "First message"),
                (1, ConversationMessageRole.Assistant, "New message")
            ]);

        Assert.Equal(2, conversation.Snapshots.Count);
    }

    [Fact]
    public void AddSnapshot_WithoutMessages_ThrowsAndDoesNotAttachSnapshot()
    {
        var conversation = new Conversation("Title", CreatedAtUtc);

        Assert.Throws<ArgumentException>(
            () => conversation.AddSnapshot(
                null,
                SourceUrl,
                CreatedAtUtc.AddMinutes(1),
                []));
        Assert.Empty(conversation.Snapshots);
    }
}
