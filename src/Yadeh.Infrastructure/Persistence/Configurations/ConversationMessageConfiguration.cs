using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yadeh.Domain.Conversations;

namespace Yadeh.Infrastructure.Persistence.Configurations;

internal sealed class ConversationMessageConfiguration
    : IEntityTypeConfiguration<ConversationMessage>
{
    public void Configure(
        EntityTypeBuilder<ConversationMessage> builder)
    {
        builder.ToTable(
            "ConversationMessages",
            tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "CK_ConversationMessages_Sequence",
                    "\"Sequence\" >= 0");
            });

        builder.HasKey(message => message.Id);

        builder.Property(message => message.Id)
            .ValueGeneratedNever();

        builder.Property(message => message.ConversationSnapshotId)
            .IsRequired();

        builder.Property(message => message.Sequence)
            .IsRequired();

        builder.Property(message => message.Role)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(message => message.Content)
            .IsRequired();

        builder.HasIndex(message => new
        {
            message.ConversationSnapshotId,
            message.Sequence
        }).IsUnique();
    }
}