using GameCatalogue.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameCatalogue.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="OutboxMessage"/> entity.
/// </summary>
public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Type)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(m => m.Content)
            .IsRequired();

        builder.Property(m => m.OccurredAt)
            .IsRequired();

        builder.Property(m => m.ProcessedAt);
        builder.Property(m => m.Error);

        // Index to quickly find unprocessed messages.
        builder.HasIndex(m => m.ProcessedAt);
    }
}
