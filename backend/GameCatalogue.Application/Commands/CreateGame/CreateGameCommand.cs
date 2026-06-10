using GameCatalogue.Domain.Enums;
using MediatR;

namespace GameCatalogue.Application.Commands.CreateGame;

/// <summary>
/// Command to create a new game. Returns the identifier of the created game.
/// Optionally carries a cover image that is uploaded during creation.
/// </summary>
public record CreateGameCommand(
    string Title,
    Genre Genre,
    Platform Platform,
    DateOnly ReleaseDate,
    string Developer,
    decimal Rating,
    long DownloadCount,
    Stream? ImageStream = null,
    string? ImageFileName = null,
    string? ImageContentType = null) : IRequest<Guid>;
