namespace GameCatalogue.Application.Interfaces.Storage;

/// <summary>
/// Resizes raster images, preserving aspect ratio and format.
/// </summary>
public interface IImageResizer
{
    /// <summary>
    /// Resizes an image down to the given width (height scaled to keep aspect
    /// ratio). Images already narrower than <paramref name="width"/> are returned
    /// unchanged (no upscaling).
    /// </summary>
    /// <param name="content">The original image bytes.</param>
    /// <param name="width">The target width in pixels.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>The resized image bytes in the original format.</returns>
    Task<byte[]> ResizeToWidthAsync(byte[] content, int width, CancellationToken ct);
}
