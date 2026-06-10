using FluentAssertions;
using GameCatalogue.Application.Commands.DeleteGame;
using GameCatalogue.Application.Exceptions;
using GameCatalogue.Application.Interfaces.Storage;
using GameCatalogue.Domain.Entities;
using GameCatalogue.Domain.Enums;
using GameCatalogue.Domain.Events;
using GameCatalogue.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace GameCatalogue.Tests.Application;

public class DeleteGameCommandHandlerTests
{
    private readonly Mock<IGameWriteRepository> _repository = new();
    private readonly Mock<IStorageService> _storage = new();
    private readonly Mock<IPublisher> _publisher = new();
    private readonly Mock<ILogger<DeleteGameCommandHandler>> _logger = new();

    private DeleteGameCommandHandler CreateHandler() =>
        new(_repository.Object, _storage.Object, _publisher.Object, _logger.Object);

    private static Game NewGame() => Game.Create(
        "Doom", Genre.Action, Platform.PC, new DateOnly(2016, 5, 13), "id Software", 8.7m, 100);

    [Fact]
    public async Task Handle_WhenGameNotFound_ShouldThrowNotFoundException()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Game?)null);

        var act = () => CreateHandler().Handle(new DeleteGameCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        _repository.Verify(r => r.DeleteAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenGameExists_ShouldDeleteAndPublishGameDeletedEvent()
    {
        var game = NewGame();
        game.ClearDomainEvents();
        _repository.Setup(r => r.GetByIdAsync(game.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);

        await CreateHandler().Handle(new DeleteGameCommand(game.Id), CancellationToken.None);

        _repository.Verify(r => r.DeleteAsync(game, It.IsAny<CancellationToken>()), Times.Once);
        _publisher.Verify(
            p => p.Publish(It.Is<IDomainEvent>(e => e is GameDeletedEvent), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenGameHasCoverImage_ShouldDeleteImageFromStorage()
    {
        var game = NewGame();
        game.SetCoverImageKey("some-key_cover.png");
        _repository.Setup(r => r.GetByIdAsync(game.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);

        await CreateHandler().Handle(new DeleteGameCommand(game.Id), CancellationToken.None);

        _storage.Verify(s => s.DeleteAsync("some-key_cover.png", It.IsAny<CancellationToken>()), Times.Once);
    }
}
