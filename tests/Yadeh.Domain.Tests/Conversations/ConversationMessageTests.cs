using Yadeh.Domain.Conversations;
using Yadeh.Domain.Conversations.Enums;

namespace Yadeh.Domain.Tests.Conversations;

public sealed class ConversationMessageTests
{
    private static readonly DateTimeOffset CreatedAtUtc =
        new(2026, 9, 12, 8, 30, 0, TimeSpan.Zero);

    private static readonly Uri SourceUrl =
        new("https://chatgpt.com/share/conversation-id");

    [Fact]
    public void AddSnapshot_WithValidMessage_CreatesMessage()
    {
        var conversation = new Conversation("Title", CreatedAtUtc);

        var snapshot = conversation.AddSnapshot(
            null,
            SourceUrl,
            CreatedAtUtc.AddMinutes(1),
            [(3, ConversationMessageRole.Assistant, "  Answer  ")]);

        var message = Assert.Single(snapshot.Messages);

        Assert.NotEqual(Guid.Empty, message.Id);
        Assert.Equal(snapshot.Id, message.ConversationSnapshotId);
        Assert.Equal(3, message.Sequence);
        Assert.Equal(ConversationMessageRole.Assistant, message.Role);
        Assert.Equal("  Answer  ", message.Content);
    }

    [Fact]
    public void AddSnapshot_WithNegativeMessageSequence_ThrowsException()
    {
        var conversation = new Conversation("Title", CreatedAtUtc);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => conversation.AddSnapshot(
                null,
                SourceUrl,
                CreatedAtUtc.AddMinutes(1),
                [(-1, ConversationMessageRole.User, "Message")]));
        Assert.Empty(conversation.Snapshots);
    }

    [Fact]
    public void AddSnapshot_WithUndefinedMessageRole_ThrowsException()
    {
        var conversation = new Conversation("Title", CreatedAtUtc);
        var undefinedRole = (ConversationMessageRole)999;

        Assert.Throws<ArgumentOutOfRangeException>(
            () => conversation.AddSnapshot(
                null,
                SourceUrl,
                CreatedAtUtc.AddMinutes(1),
                [(0, undefinedRole, "Message")]));
        Assert.Empty(conversation.Snapshots);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddSnapshot_WithInvalidMessageContent_ThrowsException(
        string? content)
    {
        var conversation = new Conversation("Title", CreatedAtUtc);

        Assert.ThrowsAny<ArgumentException>(
            () => conversation.AddSnapshot(
                null,
                SourceUrl,
                CreatedAtUtc.AddMinutes(1),
                [(0, ConversationMessageRole.User, content!)]));
        Assert.Empty(conversation.Snapshots);
    }
}
