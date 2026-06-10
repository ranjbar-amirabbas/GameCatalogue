using GameCatalogue.Domain.Entities;
using GameCatalogue.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameCatalogue.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="Game"/> entity.
/// </summary>
public class GameConfiguration : IEntityTypeConfiguration<Game>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Game> builder)
    {
        builder.ToTable("Games");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(g => g.Developer)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(g => g.Rating)
            .HasPrecision(3, 1);

        builder.Property(g => g.Genre)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(g => g.Platform)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(g => g.ReleaseDate)
            .HasColumnType("date");

        builder.Property(g => g.CoverImageKey)
            .HasMaxLength(500);

        builder.Property(g => g.DownloadCount);
        builder.Property(g => g.CreatedAt);
        builder.Property(g => g.UpdatedAt);

        builder.Ignore(g => g.DomainEvents);

        builder.HasIndex(g => g.Title);
        builder.HasIndex(g => g.Genre);
        builder.HasIndex(g => g.Platform);
    }
}
