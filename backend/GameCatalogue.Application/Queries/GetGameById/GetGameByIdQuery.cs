using GameCatalogue.Application.DTOs;
using MediatR;

namespace GameCatalogue.Application.Queries.GetGameById;

/// <summary>
/// Query to retrieve a single game by its identifier.
/// </summary>
public record GetGameByIdQuery(Guid Id) : IRequest<GameDto?>;
