using Microsoft.EntityFrameworkCore;
using Yadeh.Domain.Conversations;
using Yadeh.Domain.Conversations.Enums;
using Yadeh.Infrastructure.Persistence.Repositories;
using Yadeh.Infrastructure.Persistence;

namespace Yadeh.Infrastructure.Tests.Persistence;

public sealed class ConversationRepositoryTests
{
    private static readonly DateTimeOffset BaseTimeUtc =
        new(2026, 9, 13, 8, 0, 0, TimeSpan.Zero);

    private static readonly Uri SourceUrl =
        new("https://chatgpt.com/share/conversation-id");

    [Fact]
    public async Task AddAndGetByIdAsync_PersistsCompleteConversation()
    {
        await using var database =
            await SqliteTestDatabase.CreateAsync();

        var repository = new ConversationRepository(database.Context);
        var unitOfWork = new YadehUnitOfWork(database.Context);

        var conversation = new Conversation(
            "Architecture discussion",
            BaseTimeUtc);

        conversation.AddSnapshot(
            "Later version",
            SourceUrl,
            BaseTimeUtc.AddHours(2),
            [
                (1, ConversationMessageRole.Assistant, "Answer"),
                (0, ConversationMessageRole.User, "Question")
            ]);

        conversation.AddSnapshot(
            "Earlier version",
            SourceUrl,
            BaseTimeUtc.AddHours(1),
            [(0, ConversationMessageRole.User, "First message")]);

        repository.Add(conversation);
        await unitOfWork.SaveChangesAsync();

        database.Context.ChangeTracker.Clear();

        var storedConversation =
            await repository.GetByIdAsync(conversation.Id);

        Assert.NotNull(storedConversation);
        Assert.Equal(
            "Architecture discussion",
            storedConversation.Title);

        Assert.Collection(
            storedConversation.Snapshots,
            earlierSnapshot =>
            {
                Assert.Equal(
                    "Earlier version",
                    earlierSnapshot.OriginalTitle);

                var message =
                    Assert.Single(earlierSnapshot.Messages);

                Assert.Equal(0, message.Sequence);
                Assert.Equal(
                    "First message",
                    message.Content);
            },
            laterSnapshot =>
            {
                Assert.Equal(
                    "Later version",
                    laterSnapshot.OriginalTitle);

                Assert.Collection(
                    laterSnapshot.Messages,
                    firstMessage =>
                    {
                        Assert.Equal(0, firstMessage.Sequence);
                        Assert.Equal(
                            ConversationMessageRole.User,
                            firstMessage.Role);
                    },
                    secondMessage =>
                    {
                        Assert.Equal(1, secondMessage.Sequence);
                        Assert.Equal(
                            ConversationMessageRole.Assistant,
                            secondMessage.Role);
                    });
            });
    }

    [Fact]
    public async Task FindAllBySourceUrlAsync_ReturnsEveryMatchingConversation()
    {
        await using var database =
            await SqliteTestDatabase.CreateAsync();

        var repository = new ConversationRepository(database.Context);
        var unitOfWork = new YadehUnitOfWork(database.Context);

        var olderConversation = CreateConversation(
            "Older conversation",
            BaseTimeUtc,
            SourceUrl);

        var newerConversation = CreateConversation(
            "Newer conversation",
            BaseTimeUtc.AddHours(1),
            SourceUrl);

        var unrelatedConversation = CreateConversation(
            "Unrelated conversation",
            BaseTimeUtc.AddHours(2),
            new Uri("https://chatgpt.com/share/another-id"));

        repository.Add(olderConversation);
        repository.Add(newerConversation);
        repository.Add(unrelatedConversation);

        await unitOfWork.SaveChangesAsync();

        database.Context.ChangeTracker.Clear();

        var conversations =
            await repository.FindAllBySourceUrlAsync(SourceUrl);

        Assert.Equal(
            [newerConversation.Id, olderConversation.Id],
            conversations.Select(conversation => conversation.Id));
    }

    [Fact]
    public async Task ListAsync_ReturnsNewestConversationFirst()
    {
        await using var database =
            await SqliteTestDatabase.CreateAsync();

        var repository = new ConversationRepository(database.Context);
        var unitOfWork = new YadehUnitOfWork(database.Context);

        var oldestConversation = CreateConversation(
            "Oldest",
            BaseTimeUtc,
            SourceUrl);

        var newestConversation = CreateConversation(
            "Newest",
            BaseTimeUtc.AddHours(2),
            SourceUrl);

        var middleConversation = CreateConversation(
            "Middle",
            BaseTimeUtc.AddHours(1),
            SourceUrl);

        repository.Add(oldestConversation);
        repository.Add(newestConversation);
        repository.Add(middleConversation);

        await unitOfWork.SaveChangesAsync();

        database.Context.ChangeTracker.Clear();

        var conversations = await repository.ListAsync();

        Assert.Equal(
            [
                newestConversation.Id,
                middleConversation.Id,
                oldestConversation.Id
            ],
            conversations.Select(conversation => conversation.Id));
    }

    [Fact]
    public async Task SaveChangesAsync_PersistsRenamedConversation()
    {
        await using var database =
            await SqliteTestDatabase.CreateAsync();

        var repository = new ConversationRepository(database.Context);
        var unitOfWork = new YadehUnitOfWork(database.Context);

        var conversation = CreateConversation(
            "Old title",
            BaseTimeUtc,
            SourceUrl);

        repository.Add(conversation);
        await unitOfWork.SaveChangesAsync();

        database.Context.ChangeTracker.Clear();

        var storedConversation =
            await repository.GetByIdAsync(conversation.Id);

        Assert.NotNull(storedConversation);

        storedConversation.Rename("New title");
        await unitOfWork.SaveChangesAsync();

        database.Context.ChangeTracker.Clear();

        var renamedConversation =
            await repository.GetByIdAsync(conversation.Id);

        Assert.NotNull(renamedConversation);
        Assert.Equal("New title", renamedConversation.Title);
    }

    [Fact]
    public async Task Remove_DeletesSnapshotsAndMessages()
    {
        await using var database =
            await SqliteTestDatabase.CreateAsync();

        var repository = new ConversationRepository(database.Context);
        var unitOfWork = new YadehUnitOfWork(database.Context);

        var conversation = CreateConversation(
            "Conversation",
            BaseTimeUtc,
            SourceUrl);

        repository.Add(conversation);
        await unitOfWork.SaveChangesAsync();

        database.Context.ChangeTracker.Clear();

        var storedConversation =
            await repository.GetByIdAsync(conversation.Id);

        Assert.NotNull(storedConversation);

        repository.Remove(storedConversation);
        await unitOfWork.SaveChangesAsync();

        database.Context.ChangeTracker.Clear();

        Assert.Equal(
            0,
            await database.Context.Conversations.CountAsync());

        Assert.Equal(
            0,
            await database.Context.ConversationSnapshots.CountAsync());

        Assert.Equal(
            0,
            await database.Context.ConversationMessages.CountAsync());
    }

    private static Conversation CreateConversation(
        string title,
        DateTimeOffset createdAtUtc,
        Uri sourceUrl)
    {
        var conversation = new Conversation(
            title,
            createdAtUtc);

        conversation.AddSnapshot(
            title,
            sourceUrl,
            createdAtUtc.AddMinutes(1),
            [(0, ConversationMessageRole.User, "Message")]);

        return conversation;
    }
}