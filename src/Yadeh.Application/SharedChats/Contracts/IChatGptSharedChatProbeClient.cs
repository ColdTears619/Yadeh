using Yadeh.Application.SharedChats.Models;
using Yadeh.Domain.SharedChats;

namespace Yadeh.Application.SharedChats.Contracts;

public interface IChatGptSharedChatProbeClient
{
    Task<SharedChatProbeResult> ProbeAsync(
        ChatGptSharedChatLink link,
        CancellationToken cancellationToken = default);
}