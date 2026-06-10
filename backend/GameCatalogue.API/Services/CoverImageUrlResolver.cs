using GameCatalogue.Application.Interfaces.Storage;

namespace GameCatalogue.API.Services;

/// <summary>
/// Builds cover image URLs of the form
/// <c>{apiBaseUrl}/api/v1/games/images/{fileKey}</c>, deriving the base URL from
/// the current request so the URL is reachable by whatever host the client used.
/// </summary>
public class CoverImageUrlResolver : ICoverImageUrlResolver
{
    /// <summary>The relative API address that serves cover images.</summary>
    public const string ImageRoute = "api/v1/games/images";

    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="CoverImageUrlResolver"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">Accessor for the current HTTP context.</param>
    public CoverImageUrlResolver(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public string Resolve(string? coverImageKey, int? width = null)
    {
        if (string.IsNullOrWhiteSpace(coverImageKey))
        {
            return string.Empty;
        }

        var encodedKey = Uri.EscapeDataString(coverImageKey);
        var query = width is > 0 ? $"?w={width}" : string.Empty;
        var request = _httpContextAccessor.HttpContext?.Request;

        // No request context (e.g. background work): fall back to a relative URL.
        if (request is null)
        {
            return $"/{ImageRoute}/{encodedKey}{query}";
        }

        var baseUrl = $"{request.Scheme}://{request.Host.Value}";
        return $"{baseUrl}/{ImageRoute}/{encodedKey}{query}";
    }
}
