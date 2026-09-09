using System.Diagnostics.CodeAnalysis;

namespace Yadeh.Domain.SharedChats;

public sealed record ChatGptSharedChatLink
{
    public Uri Value { get; }

    private ChatGptSharedChatLink(Uri value)
    {
        Value = value;
    }

    public static bool TryCreate(
        string? rawValue,
        [NotNullWhen(true)] out ChatGptSharedChatLink? link)
    {
        link = null;

        if (!Uri.TryCreate(rawValue, UriKind.Absolute, out var uri))
        {
            return false;
        }

        if (uri.Scheme != Uri.UriSchemeHttps)
        {
            return false;
        }

        if (!uri.Host.Equals(
                "chatgpt.com",
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!uri.IsDefaultPort || !string.IsNullOrEmpty(uri.UserInfo))
        {
            return false;
        }

        var pathSegments = uri.AbsolutePath.Split(
            '/',
            StringSplitOptions.RemoveEmptyEntries);

        if (pathSegments.Length != 2 ||
            !pathSegments[0].Equals(
                "share",
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        link = new ChatGptSharedChatLink(uri);
        return true;
    }

    public override string ToString()
    {
        return Value.AbsoluteUri;
    }
}