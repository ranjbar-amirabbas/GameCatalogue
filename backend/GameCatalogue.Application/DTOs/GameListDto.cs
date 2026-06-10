using GameCatalogue.Domain.Enums;

namespace GameCatalogue.Application.DTOs;

/// <summary>
/// Lightweight representation of a game used in list/browse views.
/// </summary>
public record GameListDto(
    Guid Id,
    string Title,
    Genre Genre,
    Platform Platform,
    decimal Rating,
    string CoverImageUrl);
