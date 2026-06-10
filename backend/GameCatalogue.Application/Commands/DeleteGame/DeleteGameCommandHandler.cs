using GameCatalogue.Application.Exceptions;
using GameCatalogue.Application.Interfaces.Storage;
using GameCatalogue.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GameCatalogue.Application.Commands.DeleteGame;

/// <summary>
/// Handles <see cref="DeleteGameCommand"/> by loading and deleting the game,
/// removing its cover image from storage and publishing its domain events.
/// </summary>
public class DeleteGameCommandHandler : IRequestHandler<DeleteGameCommand>
{
    private readonly IGameWriteRepository _repository;
    private readonly IStorageService _storageService;
    private readonly IPublisher _publisher;
    private readonly ILogger<DeleteGameCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteGameCommandHandler"/> class.
    /// </summary>
    public DeleteGameCommandHandler(
        IGameWriteRepository repository,
        IStorageService storageService,
        IPublisher publisher,
        ILogger<DeleteGameCommandHandler> logger)
    {
        _repository = repository;
        _storageService = storageService;
        _publisher = publisher;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task Handle(DeleteGameCommand request, CancellationToken cancellationToken)
    {
        var game = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Game), request.Id);

        game.MarkAsDeleted();

        // Capture events before saving (see CreateGameCommandHandler for rationale).
        var domainEvents = game.DomainEvents.ToList();
        var coverImageKey = game.CoverImageKey;

        await _repository.DeleteAsync(game, cancellationToken);

        // Best-effort cleanup of the orphaned cover image; never fail the delete.
        if (!string.IsNullOrWhiteSpace(coverImageKey))
        {
            try
            {
                await _storageService.DeleteAsync(coverImageKey, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete cover image {Key} for game {GameId}", coverImageKey, request.Id);
            }
        }

        foreach (var domainEvent in domainEvents)
        {
            await _publisher.Publish(domainEvent, cancellationToken);
        }

        _logger.LogInformation("Deleted game {GameId}", request.Id);
    }
}
