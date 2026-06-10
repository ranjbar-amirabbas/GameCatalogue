using GameCatalogue.Application.Interfaces.Storage;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;

namespace GameCatalogue.Infrastructure.Storage;

/// <summary>
/// <see cref="IImageResizer"/> implementation backed by SixLabors.ImageSharp
/// (fully managed, no native dependencies).
/// </summary>
public class ImageSharpResizer : IImageResizer
{
    private readonly ILogger<ImageSharpResizer> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageSharpResizer"/> class.
    /// </summary>
    public ImageSharpResizer(ILogger<ImageSharpResizer> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<byte[]> ResizeToWidthAsync(byte[] content, int width, CancellationToken ct)
    {
        try
        {
            using var image = Image.Load(content);

            // Don't upscale.
            if (image.Width <= width)
            {
                return content;
            }

            var format = image.Metadata.DecodedImageFormat;
            var height = (int)Math.Round(image.Height * (width / (double)image.Width));

            image.Mutate(x => x.Resize(width, Math.Max(1, height)));

            using var output = new MemoryStream();
            IImageEncoder encoder = image.Configuration.ImageFormatsManager.GetEncoder(
                format ?? SixLabors.ImageSharp.Formats.Png.PngFormat.Instance);
            await image.SaveAsync(output, encoder, ct);
            return output.ToArray();
        }
        catch (Exception ex)
        {
            // If the bytes aren't a decodable image, fall back to the original.
            _logger.LogWarning(ex, "Failed to resize image; returning original.");
            return content;
        }
    }
}
