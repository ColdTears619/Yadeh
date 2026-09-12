using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;

namespace Yadeh.Infrastructure.SharedChats.Parsing;

internal sealed class ReactRouterStreamDecoder
{
    private const string EnqueueMarker =
        "window.__reactRouterContext.streamController.enqueue(";

    public bool TryDecode(
        string html,
        [NotNullWhen(true)]
        out IReadOnlyDictionary<string, object?>? root)
    {
        root = null;

        if (string.IsNullOrWhiteSpace(html))
        {
            return false;
        }

        if (!TryExtractDataRecord(html, out var dataRecord))
        {
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(dataRecord);

            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                return false;
            }

            var table = document.RootElement
                .EnumerateArray()
                .Select(element => element.Clone())
                .ToArray();

            if (table.Length == 0)
            {
                return false;
            }

            var cache = new Dictionary<int, object?>();
            var decodedRoot = DecodeTableEntry(0, table, cache);

            root = decodedRoot as IReadOnlyDictionary<string, object?>;
            return root is not null;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool TryExtractDataRecord(
        string html,
        [NotNullWhen(true)] out string? dataRecord)
    {
        dataRecord = null;
        var searchIndex = 0;

        while (searchIndex < html.Length)
        {
            var markerIndex = html.IndexOf(
                EnqueueMarker,
                searchIndex,
                StringComparison.Ordinal);

            if (markerIndex < 0)
            {
                return false;
            }

            var argumentIndex = markerIndex + EnqueueMarker.Length;

            if (TryReadJsonString(
                    html,
                    argumentIndex,
                    out var streamPayload) &&
                streamPayload.StartsWith(
                    "[",
                    StringComparison.Ordinal))
            {
                var recordEnd = streamPayload.IndexOf('\n');

                dataRecord = recordEnd >= 0
                    ? streamPayload[..recordEnd]
                    : streamPayload;

                return true;
            }

            searchIndex = argumentIndex;
        }

        return false;
    }

    private static bool TryReadJsonString(
        string source,
        int startIndex,
        [NotNullWhen(true)] out string? value)
    {
        value = null;

        try
        {
            var remainingSource = source[startIndex..];
            var utf8Source = Encoding.UTF8.GetBytes(remainingSource);
            var reader = new Utf8JsonReader(utf8Source);

            if (!reader.Read() ||
                reader.TokenType != JsonTokenType.String)
            {
                return false;
            }

            value = reader.GetString();
            return value is not null;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static object? DecodeTableEntry(
        int index,
        IReadOnlyList<JsonElement> table,
        IDictionary<int, object?> cache)
    {
        if (index < 0 || index >= table.Count)
        {
            return null;
        }

        if (cache.TryGetValue(index, out var cachedValue))
        {
            return cachedValue;
        }

        var element = table[index];

        return element.ValueKind switch
        {
            JsonValueKind.Object =>
                DecodeObject(index, element, table, cache),

            JsonValueKind.Array =>
                DecodeArray(index, element, table, cache),

            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => DecodeNumber(element),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => null
        };
    }

    private static IReadOnlyDictionary<string, object?> DecodeObject(
        int index,
        JsonElement element,
        IReadOnlyList<JsonElement> table,
        IDictionary<int, object?> cache)
    {
        var result = new Dictionary<string, object?>();
        cache[index] = result;

        foreach (var property in element.EnumerateObject())
        {
            var propertyName = DecodePropertyName(
                property.Name,
                table,
                cache);

            result[propertyName] = DecodeReferenceOrValue(
                property.Value,
                table,
                cache);
        }

        return result;
    }

    private static IReadOnlyList<object?> DecodeArray(
        int index,
        JsonElement element,
        IReadOnlyList<JsonElement> table,
        IDictionary<int, object?> cache)
    {
        var result = new List<object?>();
        cache[index] = result;

        foreach (var item in element.EnumerateArray())
        {
            result.Add(DecodeReferenceOrValue(
                item,
                table,
                cache));
        }

        return result;
    }

    private static string DecodePropertyName(
        string encodedName,
        IReadOnlyList<JsonElement> table,
        IDictionary<int, object?> cache)
    {
        if (encodedName.Length > 1 &&
            encodedName[0] == '_' &&
            int.TryParse(encodedName.AsSpan(1), out var keyIndex) &&
            DecodeTableEntry(keyIndex, table, cache) is string key)
        {
            return key;
        }

        return encodedName;
    }

    private static object? DecodeReferenceOrValue(
        JsonElement element,
        IReadOnlyList<JsonElement> table,
        IDictionary<int, object?> cache)
    {
        if (element.ValueKind == JsonValueKind.Number &&
            element.TryGetInt32(out var reference))
        {
            return reference >= 0
                ? DecodeTableEntry(reference, table, cache)
                : DecodeSpecialValue(reference);
        }

        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Number => DecodeNumber(element),
            _ => null
        };
    }

    private static object? DecodeNumber(JsonElement element)
    {
        return element.TryGetInt64(out var integer)
            ? integer
            : element.GetDouble();
    }

    private static object? DecodeSpecialValue(int value)
    {
        return value switch
        {
            -3 => double.NaN,
            -4 => double.PositiveInfinity,
            -5 => double.NegativeInfinity,
            _ => null
        };
    }
}