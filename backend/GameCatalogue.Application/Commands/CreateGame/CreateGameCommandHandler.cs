using GameCatalogue.Application.Interfaces.Storage;
using GameCatalogue.Domain.Entities;
using GameCatalogue.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GameCatalogue.Application.Commands.CreateGame;

/// <summary>
/// Handles <see cref="CreateGameCommand"/> by creating and persisting a new game
/// (optionally uploading its cover image) and publishing its domain events.
/// </summary>
public class CreateGameCommandHandler : IRequestHandler<CreateGameCommand, Guid>
{
    private readonly IGameWriteRepository _repository;
    private readonly IStorageService _storageService;
    private readonly IPublisher _publisher;
    private readonly ILogger<CreateGameCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateGameCommandHandler"/> class.
    /// </summary>
    public CreateGameCommandHandler(
        IGameWriteRepository repository,
        IStorageService storageService,
        IPublisher publisher,
        ILogger<CreateGameCommandHandler> logger)
    {
        _repository = repository;
        _storageService = storageService;
        _publisher = publisher;
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

        // Capture events before saving: WriteDbContext moves them to the outbox
        // (for at-least-once delivery) and clears them during SaveChanges. We also
        // publish them in-process right after the commit for read-your-writes
        // consistency (e.g. immediate cache invalidation).
        var domainEvents = game.DomainEvents.ToList();

        await _repository.AddAsync(game, cancellationToken);

        foreach (var domainEvent in domainEvents)
        {
            await _publisher.Publish(domainEvent, cancellationToken);
        }

        _logger.LogInformation("Created game {GameId} with title {Title}", game.Id, game.Title);
        return game.Id;
    }
}
