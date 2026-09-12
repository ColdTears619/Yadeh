using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yadeh.Domain.Conversations;

namespace Yadeh.Infrastructure.Persistence.Configurations;

internal sealed class ConversationConfiguration
    : IEntityTypeConfiguration<Conversation>
{
    public void Configure(
        EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("Conversations");

        builder.HasKey(conversation => conversation.Id);

        builder.Property(conversation => conversation.Id)
            .ValueGeneratedNever();

        builder.Property(conversation => conversation.Title)
            .IsRequired();

        builder.Property(conversation => conversation.CreatedAtUtc)
            .HasConversion(
                value => value.UtcDateTime,
                value => new DateTimeOffset(
                    DateTime.SpecifyKind(value, DateTimeKind.Utc)))
            .IsRequired();

        builder.HasIndex(conversation => conversation.CreatedAtUtc);

        builder.HasMany(conversation => conversation.Snapshots)
            .WithOne()
            .HasForeignKey(snapshot => snapshot.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(conversation => conversation.Snapshots)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}