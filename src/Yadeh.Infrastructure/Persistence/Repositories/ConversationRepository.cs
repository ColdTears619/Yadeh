using Microsoft.EntityFrameworkCore;
using Yadeh.Application.Conversations.Contracts;
using Yadeh.Domain.Conversations;

namespace Yadeh.Infrastructure.Persistence.Repositories;

internal sealed class ConversationRepository(
    YadehDbContext dbContext)
    : IConversationRepository
{
    public Task<Conversation?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "The conversation identifier is required.",
                nameof(id));
        }

        return CompleteConversations()
            .SingleOrDefaultAsync(
                conversation => conversation.Id == id,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Conversation>>
        FindAllBySourceUrlAsync(
            Uri sourceUrl,
            CancellationToken cancellationToken = default)
    {
        var normalizedSourceUrl = NormalizeSourceUrl(sourceUrl);

        return await CompleteConversations()
            .Where(conversation =>
                conversation.Snapshots.Any(snapshot =>
                    snapshot.SourceUrl == normalizedSourceUrl))
            .OrderByDescending(conversation =>
                conversation.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Conversation>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        return await CompleteConversations()
            .OrderByDescending(conversation =>
                conversation.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public void Add(Conversation conversation)
    {
        ArgumentNullException.ThrowIfNull(conversation);

        dbContext.Conversations.Add(conversation);
    }

    public void Remove(Conversation conversation)
    {
        ArgumentNullException.ThrowIfNull(conversation);

        dbContext.Conversations.Remove(conversation);
    }

    private IQueryable<Conversation> CompleteConversations()
    {
        return dbContext.Conversations
            .Include(conversation =>
                conversation.Snapshots.OrderBy(snapshot =>
                    snapshot.ImportedAtUtc))
            .ThenInclude(snapshot =>
                snapshot.Messages.OrderBy(message =>
                    message.Sequence))
            .AsSplitQuery();
    }

    private static string NormalizeSourceUrl(Uri sourceUrl)
    {
        ArgumentNullException.ThrowIfNull(sourceUrl);

        if (!sourceUrl.IsAbsoluteUri)
        {
            throw new ArgumentException(
                "The source URL must be absolute.",
                nameof(sourceUrl));
        }

        return sourceUrl.AbsoluteUri;
    }
}