using Yadeh.Domain.SharedChats;

namespace Yadeh.Domain.Tests.SharedChats;

public sealed class ChatGptSharedChatLinkTests
{
    [Theory]
    [InlineData("https://chatgpt.com/share/conversation-id")]
    [InlineData("https://CHATGPT.COM/share/conversation-id")]
    [InlineData("https://chatgpt.com/share/conversation-id?locale=en")]
    public void TryCreate_WithValidShareUrl_ReturnsLink(
        string rawUrl)
    {
        var succeeded = ChatGptSharedChatLink.TryCreate(
            rawUrl,
            out var link);

        Assert.True(succeeded);
        Assert.NotNull(link);
        Assert.Equal(new Uri(rawUrl), link.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-url")]
    [InlineData("http://chatgpt.com/share/conversation-id")]
    [InlineData("https://example.com/share/conversation-id")]
    [InlineData("https://chatgpt.com@example.com/share/conversation-id")]
    [InlineData("https://example.com@chatgpt.com/share/conversation-id")]
    [InlineData("https://chatgpt.com:444/share/conversation-id")]
    [InlineData("https://chatgpt.com/share/")]
    [InlineData("https://chatgpt.com/share/conversation-id/extra")]
    [InlineData("https://chatgpt.com/c/conversation-id")]
    public void TryCreate_WithInvalidUrl_ReturnsFailure(
        string? rawUrl)
    {
        var succeeded = ChatGptSharedChatLink.TryCreate(
            rawUrl,
            out var link);

        Assert.False(succeeded);
        Assert.Null(link);
    }
}