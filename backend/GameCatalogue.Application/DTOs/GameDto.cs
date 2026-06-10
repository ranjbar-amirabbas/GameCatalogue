using GameCatalogue.Domain.Enums;

namespace GameCatalogue.Application.DTOs;

/// <summary>
/// Full representation of a game returned to clients.
/// </summary>
public record GameDto(
    Guid Id,
    string Title,
    Genre Genre,
    Platform Platform,
    DateOnly ReleaseDate,
    string Developer,
    decimal Rating,
    long DownloadCount,
    string CoverImageUrl,
    DateTime CreatedAt,
    DateTime UpdatedAt);
