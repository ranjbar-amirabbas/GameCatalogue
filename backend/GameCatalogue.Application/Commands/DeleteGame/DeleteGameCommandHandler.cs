using GameCatalogue.Application.Exceptions;
using GameCatalogue.Application.Interfaces.Storage;
using GameCatalogue.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GameCatalogue.Application.Commands.DeleteGame;

/// <summary>
/// Handles <see cref="DeleteGameCommand"/> by loading and deleting the game and
/// removing its cover image from storage. The <c>GameDeletedEvent</c> raised by the
/// aggregate is dispatched by the write context during <c>SaveChanges</c>.
/// </summary>
public class DeleteGameCommandHandler : IRequestHandler<DeleteGameCommand>
{
    private readonly IGameWriteRepository _repository;
    private readonly IStorageService _storageService;
    private readonly ILogger<DeleteGameCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteGameCommandHandler"/> class.
    /// </summary>
    public DeleteGameCommandHandler(
        IGameWriteRepository repository,
        IStorageService storageService,
        ILogger<DeleteGameCommandHandler> logger)
    {
        _repository = repository;
        _storageService = storageService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task Handle(DeleteGameCommand request, CancellationToken cancellationToken)
    {
        var game = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Game), request.Id);

        game.MarkAsDeleted();

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

        _logger.LogInformation("Deleted game {GameId}", request.Id);
    }
}
