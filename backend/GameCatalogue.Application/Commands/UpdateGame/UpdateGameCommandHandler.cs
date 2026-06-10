using GameCatalogue.Application.Exceptions;
using GameCatalogue.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GameCatalogue.Application.Commands.UpdateGame;

/// <summary>
/// Handles <see cref="UpdateGameCommand"/> by loading, updating and persisting a
/// game. Domain events are dispatched by the write context during <c>SaveChanges</c>.
/// </summary>
public class UpdateGameCommandHandler : IRequestHandler<UpdateGameCommand>
{
    private readonly IGameWriteRepository _repository;
    private readonly ILogger<UpdateGameCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateGameCommandHandler"/> class.
    /// </summary>
    public UpdateGameCommandHandler(
        IGameWriteRepository repository,
        ILogger<UpdateGameCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task Handle(UpdateGameCommand request, CancellationToken cancellationToken)
    {
        var game = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Game), request.Id);

        game.Update(
            request.Title,
            request.Genre,
            request.Platform,
            request.ReleaseDate,
            request.Developer,
            request.Rating,
            request.DownloadCount);

        await _repository.UpdateAsync(game, cancellationToken);

        _logger.LogInformation("Updated game {GameId}", game.Id);
    }
}
