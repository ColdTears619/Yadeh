using Yadeh.Application.SharedChats.Enums;
using Yadeh.Infrastructure.SharedChats.Parsing;

namespace Yadeh.Infrastructure.Tests.SharedChats.Parsing;

public sealed class ChatGptSharedChatPageParserTests
{
    [Fact]
    public async Task TryParse_WithValidPage_ReturnsVisibleMessages()
    {
        var html = await ReadFixtureAsync(
            "valid-share-page.html");

        var parser = new ChatGptSharedChatPageParser(
            new ReactRouterStreamDecoder());

        var succeeded = parser.TryParse(
            html,
            out var snapshot);

        Assert.True(succeeded);
        Assert.NotNull(snapshot);
        Assert.Equal("Fixture conversation", snapshot.Title);

        Assert.Collection(
            snapshot.Messages,
            first =>
            {
                Assert.Equal(0, first.Sequence);
                Assert.Equal(
                    SharedChatMessageRole.Assistant,
                    first.Role);

                Assert.Contains("سلام from assistant", first.Content);
                Assert.Contains(
                    "Console.WriteLine(\"Hello\");",
                    first.Content);
            },
            second =>
            {
                Assert.Equal(1, second.Sequence);
                Assert.Equal(
                    SharedChatMessageRole.User,
                    second.Role);

                Assert.Equal("Hello from user", second.Content);
            },
            third =>
            {
                Assert.Equal(2, third.Sequence);
                Assert.Equal(
                    SharedChatMessageRole.Assistant,
                    third.Role);

                Assert.Equal("Final answer", third.Content);
            });

        Assert.DoesNotContain(
            snapshot.Messages,
            message => message.Content.Contains(
                "Hidden",
                StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("")]
    [InlineData("<html><body>No stream data</body></html>")]
    public void TryParse_WithInvalidPage_ReturnsFailure(
        string html)
    {
        var parser = new ChatGptSharedChatPageParser(
            new ReactRouterStreamDecoder());

        var succeeded = parser.TryParse(
            html,
            out var snapshot);

        Assert.False(succeeded);
        Assert.Null(snapshot);
    }

    private static Task<string> ReadFixtureAsync(
        string fixtureName)
    {
        var fixturePath = Path.Combine(
            AppContext.BaseDirectory,
            "SharedChats",
            "Parsing",
            "Fixtures",
            fixtureName);

        return File.ReadAllTextAsync(fixturePath);
    }
}