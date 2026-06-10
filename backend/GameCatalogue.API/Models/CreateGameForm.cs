using GameCatalogue.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace GameCatalogue.API.Models;

/// <summary>
/// Multipart form payload for creating a game, including an optional cover image.
/// </summary>
public class CreateGameForm
{
    /// <summary>The title of the game.</summary>
    [FromForm] public string Title { get; set; } = string.Empty;

    /// <summary>The genre of the game.</summary>
    [FromForm] public Genre Genre { get; set; }

    /// <summary>The platform of the game.</summary>
    [FromForm] public Platform Platform { get; set; }

    /// <summary>The release date of the game.</summary>
    [FromForm] public DateOnly ReleaseDate { get; set; }

    /// <summary>The developer of the game.</summary>
    [FromForm] public string Developer { get; set; } = string.Empty;

    /// <summary>The rating of the game (0.0 - 10.0).</summary>
    [FromForm] public decimal Rating { get; set; }

    /// <summary>The number of downloads.</summary>
    [FromForm] public long DownloadCount { get; set; }

    /// <summary>Optional cover image file.</summary>
    [FromForm] public IFormFile? File { get; set; }
}
