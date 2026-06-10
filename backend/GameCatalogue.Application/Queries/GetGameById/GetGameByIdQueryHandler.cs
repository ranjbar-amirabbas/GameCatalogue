using GameCatalogue.Application.DTOs;
using GameCatalogue.Application.Interfaces.Cache;
using GameCatalogue.Application.Interfaces.Persistence;
using GameCatalogue.Application.Interfaces.Storage;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GameCatalogue.Application.Queries.GetGameById;

/// <summary>
/// Handles <see cref="GetGameByIdQuery"/> with read-through caching. The cache
/// stores the cover image storage key; the public URL is resolved on the way out
/// so cached entries are independent of the request host.
/// </summary>
public class GetGameByIdQueryHandler : IRequestHandler<GetGameByIdQuery, GameDto?>
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(10);

    private readonly IReadDbContext _context;
    private readonly ICacheService _cache;
    private readonly ICoverImageUrlResolver _urlResolver;
    private readonly ILogger<GetGameByIdQueryHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetGameByIdQueryHandler"/> class.
    /// </summary>
    public GetGameByIdQueryHandler(
        IReadDbContext context,
        ICacheService cache,
        ICoverImageUrlResolver urlResolver,
        ILogger<GetGameByIdQueryHandler> logger)
    {
        _context = context;
        _cache = cache;
        _urlResolver = urlResolver;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<GameDto?> Handle(GetGameByIdQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"game:{request.Id}";

        // The cached DTO carries the storage key in its CoverImageUrl slot.
        var cached = await _cache.GetAsync<GameDto>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            _logger.LogInformation("Cache hit for {CacheKey}", cacheKey);
            return WithResolvedUrl(cached);
        }

        _logger.LogInformation("Cache miss for {CacheKey}; querying database", cacheKey);

        var game = await _context.Games
            .Where(g => g.Id == request.Id)
            .Select(g => new GameDto(
                g.Id,
                g.Title,
                g.Genre,
                g.Platform,
                g.ReleaseDate,
                g.Developer,
                g.Rating,
                g.DownloadCount,
                g.CoverImageKey,
                g.CreatedAt,
                g.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        if (game is null)
        {
            return null;
        }

        await _cache.SetAsync(cacheKey, game, CacheTtl, cancellationToken);
        return WithResolvedUrl(game);
    }

    private GameDto WithResolvedUrl(GameDto dto) =>
        dto with { CoverImageUrl = _urlResolver.Resolve(dto.CoverImageUrl) };
}
