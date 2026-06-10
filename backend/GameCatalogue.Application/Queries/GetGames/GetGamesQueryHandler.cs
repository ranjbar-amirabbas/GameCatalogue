using GameCatalogue.Application.DTOs;
using GameCatalogue.Application.Interfaces.Cache;
using GameCatalogue.Application.Interfaces.Persistence;
using GameCatalogue.Application.Interfaces.Storage;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GameCatalogue.Application.Queries.GetGames;

/// <summary>
/// Handles <see cref="GetGamesQuery"/> with read-through caching. The cache
/// stores cover image storage keys; public URLs are resolved on the way out.
/// </summary>
public class GetGamesQueryHandler : IRequestHandler<GetGamesQuery, PagedResult<GameListDto>>
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    // Width (px) of cover thumbnails served to the list view for fast loading.
    private const int ThumbnailWidth = 160;

    private readonly IReadDbContext _context;
    private readonly ICacheService _cache;
    private readonly ICoverImageUrlResolver _urlResolver;
    private readonly ILogger<GetGamesQueryHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetGamesQueryHandler"/> class.
    /// </summary>
    public GetGamesQueryHandler(
        IReadDbContext context,
        ICacheService cache,
        ICoverImageUrlResolver urlResolver,
        ILogger<GetGamesQueryHandler> logger)
    {
        _context = context;
        _cache = cache;
        _urlResolver = urlResolver;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<PagedResult<GameListDto>> Handle(GetGamesQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize;

        var cacheKey = BuildCacheKey(request, page, pageSize);

        var cached = await _cache.GetAsync<PagedResult<GameListDto>>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            _logger.LogInformation("Cache hit for {CacheKey}", cacheKey);
            return WithResolvedUrls(cached);
        }

        _logger.LogInformation("Cache miss for {CacheKey}; querying database", cacheKey);

        var query = _context.Games;

        if (request.Genre is not null)
        {
            query = query.Where(g => g.Genre == request.Genre);
        }

        if (request.Platform is not null)
        {
            query = query.Where(g => g.Platform == request.Platform);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(g =>
                g.Title.Contains(term) || g.Developer.Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(g => g.Rating)
            .ThenBy(g => g.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(g => new GameListDto(
                g.Id,
                g.Title,
                g.Genre,
                g.Platform,
                g.Rating,
                g.CoverImageKey))
            .ToListAsync(cancellationToken);

        var result = new PagedResult<GameListDto>(items, totalCount, page, pageSize);

        await _cache.SetAsync(cacheKey, result, CacheTtl, cancellationToken);

        return WithResolvedUrls(result);
    }

    private PagedResult<GameListDto> WithResolvedUrls(PagedResult<GameListDto> result)
    {
        var items = result.Items
            .Select(i => i with { CoverImageUrl = _urlResolver.Resolve(i.CoverImageUrl, ThumbnailWidth) })
            .ToList();
        return result with { Items = items };
    }

    private static string BuildCacheKey(GetGamesQuery request, int page, int pageSize)
    {
        var genre = request.Genre?.ToString() ?? "any";
        var platform = request.Platform?.ToString() ?? "any";
        var search = string.IsNullOrWhiteSpace(request.SearchTerm) ? "none" : request.SearchTerm.Trim().ToLowerInvariant();
        return $"games:page:{page}:size:{pageSize}:genre:{genre}:platform:{platform}:search:{search}";
    }
}
