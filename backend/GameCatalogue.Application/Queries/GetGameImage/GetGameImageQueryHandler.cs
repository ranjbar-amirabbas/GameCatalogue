using GameCatalogue.Application.Interfaces.Cache;
using GameCatalogue.Application.Interfaces.Storage;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GameCatalogue.Application.Queries.GetGameImage;

/// <summary>
/// Handles <see cref="GetGameImageQuery"/> by streaming the image from storage,
/// resizing to the requested width when asked and caching resized thumbnails.
/// </summary>
public class GetGameImageQueryHandler : IRequestHandler<GetGameImageQuery, StoredFile?>
{
    private static readonly TimeSpan ThumbnailCacheTtl = TimeSpan.FromHours(1);
    private const int MaxWidth = 2000;

    private readonly IStorageService _storageService;
    private readonly IImageResizer _resizer;
    private readonly ICacheService _cache;
    private readonly ILogger<GetGameImageQueryHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetGameImageQueryHandler"/> class.
    /// </summary>
    public GetGameImageQueryHandler(
        IStorageService storageService,
        IImageResizer resizer,
        ICacheService cache,
        ILogger<GetGameImageQueryHandler> logger)
    {
        _storageService = storageService;
        _resizer = resizer;
        _cache = cache;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<StoredFile?> Handle(GetGameImageQuery request, CancellationToken cancellationToken)
    {
        // Full-size image: stream straight from storage.
        if (request.Width is not > 0)
        {
            _logger.LogInformation("Loading full image {FileKey}", request.FileKey);
            return await _storageService.DownloadAsync(request.FileKey, cancellationToken);
        }

        var width = Math.Min(request.Width.Value, MaxWidth);
        var cacheKey = $"image:{request.FileKey}:w{width}";

        var cachedThumb = await _cache.GetAsync<StoredFile>(cacheKey, cancellationToken);
        if (cachedThumb is not null)
        {
            _logger.LogInformation("Thumbnail cache hit for {CacheKey}", cacheKey);
            return cachedThumb;
        }

        var original = await _storageService.DownloadAsync(request.FileKey, cancellationToken);
        if (original is null)
        {
            return null;
        }

        var resizedBytes = await _resizer.ResizeToWidthAsync(original.Content, width, cancellationToken);
        var thumbnail = original with { Content = resizedBytes };

        await _cache.SetAsync(cacheKey, thumbnail, ThumbnailCacheTtl, cancellationToken);
        _logger.LogInformation(
            "Resized {FileKey} to width {Width} ({Original} -> {Resized} bytes)",
            request.FileKey, width, original.Content.Length, resizedBytes.Length);

        return thumbnail;
    }
}
