using GameCatalogue.Application.Exceptions;
using GameCatalogue.Application.Interfaces.Storage;
using GameCatalogue.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GameCatalogue.Application.Commands.UpdateCoverImage;

/// <summary>
/// Handles <see cref="UpdateCoverImageCommand"/> by uploading the image to object
/// storage and assigning the returned URL to the game.
/// </summary>
public class UpdateCoverImageCommandHandler : IRequestHandler<UpdateCoverImageCommand, string>
{
    private readonly IGameWriteRepository _repository;
    private readonly IStorageService _storageService;
    private readonly ICoverImageUrlResolver _urlResolver;
    private readonly ILogger<UpdateCoverImageCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateCoverImageCommandHandler"/> class.
    /// </summary>
    public UpdateCoverImageCommandHandler(
        IGameWriteRepository repository,
        IStorageService storageService,
        ICoverImageUrlResolver urlResolver,
        ILogger<UpdateCoverImageCommandHandler> logger)
    {
        _repository = repository;
        _storageService = storageService;
        _urlResolver = urlResolver;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<string> Handle(UpdateCoverImageCommand request, CancellationToken cancellationToken)
    {
        var game = await _repository.GetByIdAsync(request.GameId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Game), request.GameId);

        var fileKey = await _storageService.UploadAsync(
            request.ImageStream,
            request.FileName,
            request.ContentType,
            cancellationToken);

        game.SetCoverImageKey(fileKey);
        await _repository.UpdateAsync(game, cancellationToken);

        _logger.LogInformation("Updated cover image for game {GameId}: {FileKey}", game.Id, fileKey);
        return _urlResolver.Resolve(fileKey);
    }
}
