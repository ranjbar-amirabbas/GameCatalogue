using GameCatalogue.Application.DTOs;
using GameCatalogue.Domain.Enums;
using MediatR;

namespace GameCatalogue.Application.Queries.GetGames;

/// <summary>
/// Query to retrieve a filtered, paginated list of games.
/// </summary>
public record GetGamesQuery(
    int Page = 1,
    int PageSize = 10,
    Genre? Genre = null,
    Platform? Platform = null,
    string? SearchTerm = null) : IRequest<PagedResult<GameListDto>>;
