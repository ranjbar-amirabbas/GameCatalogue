using FluentAssertions;
using GameCatalogue.Application.Commands.CreateGame;
using GameCatalogue.Application.Interfaces.Storage;
using GameCatalogue.Domain.Entities;
using GameCatalogue.Domain.Enums;
using GameCatalogue.Domain.Events;
using GameCatalogue.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace GameCatalogue.Tests.Application;

public class CreateGameCommandHandlerTests
{
    private readonly Mock<IGameWriteRepository> _repository = new();
    private readonly Mock<IStorageService> _storage = new();
    private readonly Mock<IPublisher> _publisher = new();
    private readonly Mock<ILogger<CreateGameCommandHandler>> _logger = new();

    private CreateGameCommandHandler CreateHandler() =>
        new(_repository.Object, _storage.Object, _publisher.Object, _logger.Object);

    private static CreateGameCommand ValidCommand() => new(
        Title: "Halo Infinite",
        Genre: Genre.Action,
        Platform: Platform.Xbox,
        ReleaseDate: new DateOnly(2021, 12, 8),
        Developer: "343 Industries",
        Rating: 8.5m,
        DownloadCount: 750_000);

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnGameId()
    {
        var handler = CreateHandler();

        var id = await handler.Handle(ValidCommand(), CancellationToken.None);

        id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCallRepositoryAddAsync()
    {
        var handler = CreateHandler();

        await handler.Handle(ValidCommand(), CancellationToken.None);

        _repository.Verify(
            r => r.AddAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldPublishGameCreatedEvent()
    {
        var handler = CreateHandler();

        await handler.Handle(ValidCommand(), CancellationToken.None);

        _publisher.Verify(
            p => p.Publish(
                It.Is<IDomainEvent>(e => e is GameCreatedEvent),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
