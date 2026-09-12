using Microsoft.EntityFrameworkCore;
using Yadeh.Domain.Conversations;

namespace Yadeh.Infrastructure.Persistence;

public sealed class YadehDbContext(
    DbContextOptions<YadehDbContext> options)
    : DbContext(options)
{
    public DbSet<Conversation> Conversations =>
        Set<Conversation>();

    public DbSet<ConversationSnapshot> ConversationSnapshots =>
        Set<ConversationSnapshot>();

    public DbSet<ConversationMessage> ConversationMessages =>
        Set<ConversationMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(YadehDbContext).Assembly);
    }
}