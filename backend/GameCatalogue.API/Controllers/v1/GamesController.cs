using Asp.Versioning;
using GameCatalogue.API.Models;
using GameCatalogue.Application.Commands.CreateGame;
using GameCatalogue.Application.Commands.DeleteGame;
using GameCatalogue.Application.Commands.UpdateCoverImage;
using GameCatalogue.Application.Commands.UpdateGame;
using GameCatalogue.Application.DTOs;
using GameCatalogue.Application.Queries.GetGameById;
using GameCatalogue.Application.Queries.GetGameImage;
using GameCatalogue.Application.Queries.GetGames;
using GameCatalogue.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GameCatalogue.API.Controllers.v1;

/// <summary>
/// REST endpoints for managing the game catalogue.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class GamesController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GamesController"/> class.
    /// </summary>
    /// <param name="mediator">The MediatR mediator.</param>
    public GamesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Returns a filtered, paginated list of games.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<GameListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Genre? genre = null,
        [FromQuery] Platform? platform = null,
        [FromQuery] string? searchTerm = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetGamesQuery(page, pageSize, genre, platform, searchTerm), ct);
        return Ok(result);
    }

    /// <summary>Returns a single game by its identifier.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GameDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetGameByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Creates a new game. Accepts <c>multipart/form-data</c> so an optional cover
    /// image can be uploaded as part of creation.
    /// </summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromForm] CreateGameForm form, CancellationToken ct)
    {
        Stream? imageStream = null;
        try
        {
            if (form.File is { Length: > 0 })
            {
                imageStream = form.File.OpenReadStream();
            }

            var command = new CreateGameCommand(
                form.Title,
                form.Genre,
                form.Platform,
                form.ReleaseDate,
                form.Developer,
                form.Rating,
                form.DownloadCount,
                imageStream,
                form.File?.FileName,
                form.File?.ContentType);

            var id = await _mediator.Send(command, ct);
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }
        finally
        {
            if (imageStream is not null)
            {
                await imageStream.DisposeAsync();
            }
        }
    }

    /// <summary>
    /// Serves a cover image by its storage key. Pass <c>?w=</c> to get a resized
    /// thumbnail (used by the list view for faster loading).
    /// </summary>
    [HttpGet("images/{fileKey}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetImage(string fileKey, [FromQuery] int? w, CancellationToken ct)
    {
        var image = await _mediator.Send(new GetGameImageQuery(fileKey, w), ct);
        if (image is null)
        {
            return NotFound();
        }

        // Images are immutable (unique key per upload), so allow long browser caching.
        Response.Headers.CacheControl = "public, max-age=86400";
        return File(image.Content, image.ContentType);
    }

    /// <summary>Updates an existing game.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGameCommand command, CancellationToken ct)
    {
        await _mediator.Send(command with { Id = id }, ct);
        return NoContent();
    }

    /// <summary>Deletes a game.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteGameCommand(id), ct);
        return NoContent();
    }

    /// <summary>Uploads and assigns a cover image to a game.</summary>
    [HttpPost("{id:guid}/cover-image")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadCoverImage(Guid id, IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { error = "A non-empty file is required." });
        }

        await using var stream = file.OpenReadStream();
        var url = await _mediator.Send(
            new UpdateCoverImageCommand(id, stream, file.FileName, file.ContentType), ct);

        return Ok(new { url });
    }
}
