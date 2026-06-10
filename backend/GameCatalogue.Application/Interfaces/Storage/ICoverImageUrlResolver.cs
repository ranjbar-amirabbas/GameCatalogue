namespace GameCatalogue.Application.Interfaces.Storage;

/// <summary>
/// Builds the public URL for a cover image from its storage key. The URL points
/// at the image-serving API endpoint (<c>{apiBaseUrl}/api/v1/games/images/{fileKey}</c>).
/// </summary>
public interface ICoverImageUrlResolver
{
    /// <summary>
    /// Resolves a cover image storage key into a publicly accessible URL,
    /// optionally requesting a resized thumbnail of the given width.
    /// </summary>
    /// <param name="coverImageKey">The storage object key (may be null/empty).</param>
    /// <param name="width">Optional thumbnail width in pixels.</param>
    /// <returns>The image URL, or an empty string when no key is set.</returns>
    string Resolve(string? coverImageKey, int? width = null);
}
