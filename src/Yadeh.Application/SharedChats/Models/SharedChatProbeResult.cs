namespace Yadeh.Application.SharedChats.Models;

public sealed record SharedChatProbeResult
{
    public bool IsSuccess { get; }

    public SharedChatSnapshot? Snapshot { get; }

    public int DownloadedContentLength { get; }

    public string? Error { get; }

    private SharedChatProbeResult(
        bool isSuccess,
        SharedChatSnapshot? snapshot,
        int downloadedContentLength,
        string? error)
    {
        IsSuccess = isSuccess;
        Snapshot = snapshot;
        DownloadedContentLength = downloadedContentLength;
        Error = error;
    }

    public static SharedChatProbeResult Success(
        SharedChatSnapshot snapshot,
        int downloadedContentLength)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentOutOfRangeException.ThrowIfNegative(downloadedContentLength);

        return new SharedChatProbeResult(
            true,
            snapshot,
            downloadedContentLength,
            null);
    }

    public static SharedChatProbeResult Failure(string error)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(error);

        return new SharedChatProbeResult(
            false,
            null,
            0,
            error);
    }
}