using MediatR;

namespace GameCatalogue.Application.Commands.UpdateCoverImage;

/// <summary>
/// Command to upload and assign a cover image to a game. Returns the public URL
/// of the uploaded image.
/// </summary>
public record UpdateCoverImageCommand(
    Guid GameId,
    Stream ImageStream,
    string FileName,
    string ContentType) : IRequest<string>;
