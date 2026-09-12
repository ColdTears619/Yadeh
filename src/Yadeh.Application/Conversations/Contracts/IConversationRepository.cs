using Yadeh.Domain.Conversations;

namespace Yadeh.Application.Conversations.Contracts;

public interface IConversationRepository
{
    Task<Conversation?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Conversation>> FindAllBySourceUrlAsync(
        Uri sourceUrl,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Conversation>> ListAsync(
        CancellationToken cancellationToken = default);

    void Add(Conversation conversation);

    void Remove(Conversation conversation);
}