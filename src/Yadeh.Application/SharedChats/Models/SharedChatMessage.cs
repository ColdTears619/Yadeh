using Yadeh.Application.SharedChats.Enums;

namespace Yadeh.Application.SharedChats.Models;

public sealed record SharedChatMessage
{
    public int Sequence { get; }

    public SharedChatMessageRole Role { get; }

    public string Content { get; }

    public SharedChatMessage(
        int sequence,
        SharedChatMessageRole role,
        string content)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(sequence);

        if (!Enum.IsDefined(role))
        {
            throw new ArgumentOutOfRangeException(nameof(role));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        Sequence = sequence;
        Role = role;
        Content = content;
    }
}