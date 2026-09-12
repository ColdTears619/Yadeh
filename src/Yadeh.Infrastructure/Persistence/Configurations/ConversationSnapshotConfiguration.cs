using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yadeh.Domain.Conversations;

namespace Yadeh.Infrastructure.Persistence.Configurations;

internal sealed class ConversationSnapshotConfiguration
    : IEntityTypeConfiguration<ConversationSnapshot>
{
    public void Configure(
        EntityTypeBuilder<ConversationSnapshot> builder)
    {
        builder.ToTable("ConversationSnapshots");

        builder.HasKey(snapshot => snapshot.Id);

        builder.Property(snapshot => snapshot.Id)
            .ValueGeneratedNever();

        builder.Property(snapshot => snapshot.ConversationId)
            .IsRequired();

        builder.Property(snapshot => snapshot.OriginalTitle);

        builder.Property(snapshot => snapshot.SourceUrl)
            .IsRequired();

        builder.Property(snapshot => snapshot.ImportedAtUtc)
            .HasConversion(
                value => value.UtcDateTime,
                value => new DateTimeOffset(
                    DateTime.SpecifyKind(value, DateTimeKind.Utc)))
            .IsRequired();

        builder.HasIndex(snapshot => snapshot.SourceUrl);

        builder.HasIndex(snapshot => new
        {
            snapshot.ConversationId,
            snapshot.ImportedAtUtc
        });

        builder.HasMany(snapshot => snapshot.Messages)
            .WithOne()
            .HasForeignKey(message => message.ConversationSnapshotId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(snapshot => snapshot.Messages)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}