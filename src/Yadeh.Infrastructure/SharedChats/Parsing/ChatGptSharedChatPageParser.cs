using System.Diagnostics.CodeAnalysis;
using Yadeh.Application.SharedChats.Enums;
using Yadeh.Application.SharedChats.Models;

namespace Yadeh.Infrastructure.SharedChats.Parsing;

internal sealed class ChatGptSharedChatPageParser(
    ReactRouterStreamDecoder streamDecoder)
    : ISharedChatPageParser
{
    private const string ShareRoutePrefix = "routes/share.";

    public bool TryParse(
        string html,
        [NotNullWhen(true)] out SharedChatSnapshot? snapshot)
    {
        snapshot = null;

        if (!streamDecoder.TryDecode(html, out var root))
        {
            return false;
        }

        if (!TryGetDictionary(root, "loaderData", out var loaderData) ||
            !TryFindShareRoute(loaderData, out var shareRoute) ||
            !TryGetDictionary(
                shareRoute,
                "serverResponse",
                out var serverResponse) ||
            !TryGetDictionary(serverResponse, "data", out var data) ||
            !TryGetList(
                data,
                "linear_conversation",
                out var linearConversation))
        {
            return false;
        }

        var title = TryGetString(data, "title", out var parsedTitle)
            ? parsedTitle
            : null;

        var startIndex = FindStartIndex(data, linearConversation);

        if (startIndex < 0)
        {
            return false;
        }

        var messages = ParseMessages(
            linearConversation,
            startIndex);

        if (messages.Count == 0)
        {
            return false;
        }

        snapshot = new SharedChatSnapshot(title, messages);
        return true;
    }

    private static bool TryFindShareRoute(
        IReadOnlyDictionary<string, object?> loaderData,
        [NotNullWhen(true)]
        out IReadOnlyDictionary<string, object?>? shareRoute)
    {
        shareRoute = null;

        foreach (var item in loaderData)
        {
            if (!item.Key.StartsWith(
                    ShareRoutePrefix,
                    StringComparison.Ordinal))
            {
                continue;
            }

            if (item.Value is IReadOnlyDictionary<string, object?> route)
            {
                shareRoute = route;
                return true;
            }
        }

        return false;
    }

    private static int FindStartIndex(
        IReadOnlyDictionary<string, object?> data,
        IReadOnlyList<object?> conversation)
    {
        if (!TryGetString(
                data,
                "highlighted_message_id",
                out var highlightedMessageId))
        {
            return 0;
        }

        for (var index = 0; index < conversation.Count; index++)
        {
            if (conversation[index]
                    is IReadOnlyDictionary<string, object?> item &&
                TryGetString(item, "id", out var messageId) &&
                messageId.Equals(
                    highlightedMessageId,
                    StringComparison.Ordinal))
            {
                return index;
            }
        }

        return -1;
    }

    private static IReadOnlyList<SharedChatMessage> ParseMessages(
        IReadOnlyList<object?> conversation,
        int startIndex)
    {
        var messages = new List<SharedChatMessage>();

        for (var index = startIndex;
             index < conversation.Count;
             index++)
        {
            if (!TryParseMessage(
                    conversation[index],
                    messages.Count,
                    out var message))
            {
                continue;
            }

            messages.Add(message);
        }

        return messages;
    }

    private static bool TryParseMessage(
        object? value,
        int sequence,
        [NotNullWhen(true)] out SharedChatMessage? message)
    {
        message = null;

        if (value is not IReadOnlyDictionary<string, object?> item ||
            !TryGetDictionary(item, "message", out var messageData) ||
            !TryGetDictionary(messageData, "author", out var author) ||
            !TryGetString(author, "role", out var rawRole) ||
            !TryMapRole(rawRole, out var role) ||
            !TryGetDictionary(messageData, "content", out var content) ||
            !TryGetString(
                content,
                "content_type",
                out var contentType) ||
            !contentType.Equals(
                "text",
                StringComparison.OrdinalIgnoreCase) ||
            !TryGetList(content, "parts", out var parts))
        {
            return false;
        }

        var textParts = parts
            .OfType<string>()
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .ToArray();

        if (textParts.Length == 0)
        {
            return false;
        }

        var text = string.Join(
            Environment.NewLine,
            textParts);

        message = new SharedChatMessage(
            sequence,
            role,
            text);

        return true;
    }

    private static bool TryMapRole(
        string rawRole,
        out SharedChatMessageRole role)
    {
        if (rawRole.Equals(
                "user",
                StringComparison.OrdinalIgnoreCase))
        {
            role = SharedChatMessageRole.User;
            return true;
        }

        if (rawRole.Equals(
                "assistant",
                StringComparison.OrdinalIgnoreCase))
        {
            role = SharedChatMessageRole.Assistant;
            return true;
        }

        role = SharedChatMessageRole.Unknown;
        return false;
    }

    private static bool TryGetDictionary(
        IReadOnlyDictionary<string, object?> source,
        string key,
        [NotNullWhen(true)]
        out IReadOnlyDictionary<string, object?>? value)
    {
        value = null;

        if (!source.TryGetValue(key, out var rawValue) ||
            rawValue is not IReadOnlyDictionary<string, object?> dictionary)
        {
            return false;
        }

        value = dictionary;
        return true;
    }

    private static bool TryGetList(
        IReadOnlyDictionary<string, object?> source,
        string key,
        [NotNullWhen(true)] out IReadOnlyList<object?>? value)
    {
        value = null;

        if (!source.TryGetValue(key, out var rawValue) ||
            rawValue is not IReadOnlyList<object?> list)
        {
            return false;
        }

        value = list;
        return true;
    }

    private static bool TryGetString(
        IReadOnlyDictionary<string, object?> source,
        string key,
        [NotNullWhen(true)] out string? value)
    {
        value = null;

        if (!source.TryGetValue(key, out var rawValue) ||
            rawValue is not string stringValue ||
            string.IsNullOrWhiteSpace(stringValue))
        {
            return false;
        }

        value = stringValue;
        return true;
    }
}