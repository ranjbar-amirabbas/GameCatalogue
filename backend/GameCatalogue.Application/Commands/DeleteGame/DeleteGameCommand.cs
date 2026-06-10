using MediatR;

namespace GameCatalogue.Application.Commands.DeleteGame;

/// <summary>
/// Command to delete a game by its identifier.
/// </summary>
public record DeleteGameCommand(Guid Id) : IRequest;
