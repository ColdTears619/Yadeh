using System.Text;
using Yadeh.Application.SharedChats.Contracts;
using Yadeh.Application.SharedChats.Models;
using Yadeh.Domain.SharedChats;
using Yadeh.Infrastructure.SharedChats.Parsing;

namespace Yadeh.Infrastructure.SharedChats;

internal sealed class ChatGptSharedChatProbeClient(
    HttpClient httpClient,
    ISharedChatPageParser pageParser)
    : IChatGptSharedChatProbeClient
{
    private const int MaximumDownloadedCharacters = 2_000_000;

    public async Task<SharedChatProbeResult> ProbeAsync(
        ChatGptSharedChatLink link,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(link);

        try
        {
            using var response = await httpClient.GetAsync(
                link.Value,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return SharedChatProbeResult.Failure(
                    $"ChatGPT returned HTTP {(int)response.StatusCode}.");
            }

            if (response.Content.Headers.ContentLength
                is > MaximumDownloadedCharacters)
            {
                return SharedChatProbeResult.Failure(
                    "The shared conversation is too large to import.");
            }

            var html = await ReadContentAsync(
                response.Content,
                cancellationToken);

            if (html is null)
            {
                return SharedChatProbeResult.Failure(
                    "The shared conversation is too large to import.");
            }

            if (!pageParser.TryParse(html, out var snapshot))
            {
                return SharedChatProbeResult.Failure(
                    "The shared conversation could not be parsed.");
            }

            return SharedChatProbeResult.Success(
                snapshot,
                html.Length);
        }
        catch (OperationCanceledException)
            when (!cancellationToken.IsCancellationRequested)
        {
            return SharedChatProbeResult.Failure(
                "The request to ChatGPT timed out.");
        }
        catch (HttpRequestException)
        {
            return SharedChatProbeResult.Failure(
                "Could not reach ChatGPT.");
        }
    }

    private static async Task<string?> ReadContentAsync(
        HttpContent content,
        CancellationToken cancellationToken)
    {
        await using var stream = await content.ReadAsStreamAsync(
            cancellationToken);

        using var reader = new StreamReader(
            stream,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true);

        var result = new StringBuilder();
        var buffer = new char[8192];

        while (true)
        {
            var charactersRead = await reader.ReadAsync(
                buffer.AsMemory(),
                cancellationToken);

            if (charactersRead == 0)
            {
                return result.ToString();
            }

            if (result.Length + charactersRead >
                MaximumDownloadedCharacters)
            {
                return null;
            }

            result.Append(
                buffer,
                0,
                charactersRead);
        }
    }
}