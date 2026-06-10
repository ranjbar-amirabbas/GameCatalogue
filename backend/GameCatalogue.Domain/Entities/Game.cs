using GameCatalogue.Domain.Enums;
using GameCatalogue.Domain.Events;
using GameCatalogue.Domain.Exceptions;

namespace GameCatalogue.Domain.Entities;

/// <summary>
/// Aggregate root representing a video game in the catalogue.
/// </summary>
public sealed class Game : AggregateRoot
{
    /// <summary>Gets the unique identifier of the game.</summary>
    public Guid Id { get; private set; }

    /// <summary>Gets the title of the game.</summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>Gets the genre of the game.</summary>
    public Genre Genre { get; private set; }

    /// <summary>Gets the platform of the game.</summary>
    public Platform Platform { get; private set; }

    /// <summary>Gets the release date of the game.</summary>
    public DateOnly ReleaseDate { get; private set; }

    /// <summary>Gets the developer of the game.</summary>
    public string Developer { get; private set; } = string.Empty;

    /// <summary>Gets the rating of the game (0.0 - 10.0, one decimal place).</summary>
    public decimal Rating { get; private set; }

    /// <summary>Gets the total number of downloads for the game.</summary>
    public long DownloadCount { get; private set; }

    /// <summary>
    /// Gets the storage object key (file key) of the game's cover image.
    /// The public URL is derived from this key when projecting to a DTO.
    /// </summary>
    public string CoverImageKey { get; private set; } = string.Empty;

    /// <summary>Gets the UTC timestamp at which the game was created.</summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>Gets the UTC timestamp at which the game was last updated.</summary>
    public DateTime UpdatedAt { get; private set; }

    // Private parameterless constructor required by EF Core.
    private Game()
    {
    }

    /// <summary>
    /// Creates a new <see cref="Game"/> aggregate after validating the inputs
    /// and raises a <see cref="GameCreatedEvent"/>.
    /// </summary>
    /// <param name="title">The title of the game.</param>
    /// <param name="genre">The genre of the game.</param>
    /// <param name="platform">The platform of the game.</param>
    /// <param name="releaseDate">The release date of the game.</param>
    /// <param name="developer">The developer of the game.</param>
    /// <param name="rating">The rating of the game (0.0 - 10.0).</param>
    /// <param name="downloadCount">The number of downloads (must be non-negative).</param>
    /// <returns>The newly created <see cref="Game"/>.</returns>
    /// <exception cref="DomainException">Thrown when any input is invalid.</exception>
    public static Game Create(
        string title,
        Genre genre,
        Platform platform,
        DateOnly releaseDate,
        string developer,
        decimal rating,
        long downloadCount)
    {
        Validate(title, developer, releaseDate, rating, downloadCount);

        var now = DateTime.UtcNow;
        var game = new Game
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
            Genre = genre,
            Platform = platform,
            ReleaseDate = releaseDate,
            Developer = developer.Trim(),
            Rating = decimal.Round(rating, 1),
            DownloadCount = downloadCount,
            CoverImageKey = string.Empty,
            CreatedAt = now,
            UpdatedAt = now
        };

        game.RaiseDomainEvent(new GameCreatedEvent(game.Id, game.Title));
        return game;
    }

    /// <summary>
    /// Updates the mutable properties of the game after validating the inputs,
    /// raises a <see cref="GameUpdatedEvent"/> and refreshes <see cref="UpdatedAt"/>.
    /// </summary>
    /// <param name="title">The new title of the game.</param>
    /// <param name="genre">The new genre of the game.</param>
    /// <param name="platform">The new platform of the game.</param>
    /// <param name="releaseDate">The new release date of the game.</param>
    /// <param name="developer">The new developer of the game.</param>
    /// <param name="rating">The new rating of the game (0.0 - 10.0).</param>
    /// <param name="downloadCount">The new download count (must be non-negative).</param>
    /// <exception cref="DomainException">Thrown when any input is invalid.</exception>
    public void Update(
        string title,
        Genre genre,
        Platform platform,
        DateOnly releaseDate,
        string developer,
        decimal rating,
        long downloadCount)
    {
        Validate(title, developer, releaseDate, rating, downloadCount);

        Title = title.Trim();
        Genre = genre;
        Platform = platform;
        ReleaseDate = releaseDate;
        Developer = developer.Trim();
        Rating = decimal.Round(rating, 1);
        DownloadCount = downloadCount;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new GameUpdatedEvent(Id, Title));
    }

    /// <summary>
    /// Raises a <see cref="GameDeletedEvent"/> to signal that this game is being
    /// removed, so downstream side-effects (e.g. cache invalidation) can react.
    /// </summary>
    public void MarkAsDeleted()
    {
        RaiseDomainEvent(new GameDeletedEvent(Id, Title));
    }

    /// <summary>
    /// Sets the cover image storage key for the game and refreshes <see cref="UpdatedAt"/>.
    /// </summary>
    /// <param name="key">The storage object key of the uploaded cover image.</param>
    /// <exception cref="DomainException">Thrown when the key is null or empty.</exception>
    public void SetCoverImageKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new DomainException("Cover image key cannot be empty.");
        }

        CoverImageKey = key;
        UpdatedAt = DateTime.UtcNow;
    }

    private static void Validate(
        string title,
        string developer,
        DateOnly releaseDate,
        decimal rating,
        long downloadCount)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException("Title is required.");
        }

        if (title.Length > 200)
        {
            throw new DomainException("Title must not exceed 200 characters.");
        }

        if (string.IsNullOrWhiteSpace(developer))
        {
            throw new DomainException("Developer is required.");
        }

        if (developer.Length > 200)
        {
            throw new DomainException("Developer must not exceed 200 characters.");
        }

        if (rating < 0m || rating > 10m)
        {
            throw new DomainException("Rating must be between 0.0 and 10.0.");
        }

        if (downloadCount < 0)
        {
            throw new DomainException("Download count must be non-negative.");
        }

        if (releaseDate == default)
        {
            throw new DomainException("Release date is required.");
        }
    }
}
