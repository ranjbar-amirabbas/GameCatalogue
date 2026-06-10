using GameCatalogue.Application.Interfaces.Storage;
using GameCatalogue.Domain.Entities;
using GameCatalogue.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GameCatalogue.Application.Commands.CreateGame;

/// <summary>
/// Handles <see cref="CreateGameCommand"/> by creating and persisting a new game
/// (optionally uploading its cover image). Domain events are dispatched by the
/// write context during <c>SaveChanges</c>.
/// </summary>
public class CreateGameCommandHandler : IRequestHandler<CreateGameCommand, Guid>
{
    private readonly IGameWriteRepository _repository;
    private readonly IStorageService _storageService;
    private readonly ILogger<CreateGameCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateGameCommandHandler"/> class.
    /// </summary>
    public CreateGameCommandHandler(
        IGameWriteRepository repository,
        IStorageService storageService,
        ILogger<CreateGameCommandHandler> logger)
    {
        _repository = repository;
        _storageService = storageService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Guid> Handle(CreateGameCommand request, CancellationToken cancellationToken)
    {
        var game = Game.Create(
            request.Title,
            request.Genre,
            request.Platform,
            request.ReleaseDate,
            request.Developer,
            request.Rating,
            request.DownloadCount);

        if (request.ImageStream is not null)
        {
            var fileKey = await _storageService.UploadAsync(
                request.ImageStream,
                request.ImageFileName ?? "cover",
                request.ImageContentType ?? "application/octet-stream",
                cancellationToken);

            game.SetCoverImageKey(fileKey);
            _logger.LogInformation("Uploaded cover image {FileKey} for new game {GameId}", fileKey, game.Id);
        }

        // The write context persists the raised domain events to the outbox and
        // dispatches them in-process during SaveChanges.
        await _repository.AddAsync(game, cancellationToken);

        _logger.LogInformation("Created game {GameId} with title {Title}", game.Id, game.Title);
        return game.Id;
    }
}
