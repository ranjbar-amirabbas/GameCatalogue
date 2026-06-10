using GameCatalogue.Domain.Enums;
using MediatR;

namespace GameCatalogue.Application.Commands.UpdateGame;

/// <summary>
/// Command to update an existing game.
/// </summary>
public record UpdateGameCommand(
    Guid Id,
    string Title,
    Genre Genre,
    Platform Platform,
    DateOnly ReleaseDate,
    string Developer,
    decimal Rating,
    long DownloadCount) : IRequest;
