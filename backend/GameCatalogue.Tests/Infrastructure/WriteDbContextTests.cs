using FluentAssertions;
using GameCatalogue.Domain.Entities;
using GameCatalogue.Domain.Enums;
using GameCatalogue.Domain.Events;
using GameCatalogue.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace GameCatalogue.Tests.Infrastructure;

public class WriteDbContextTests
{
    private static (WriteDbContext context, Mock<IPublisher> publisher) CreateContext()
    {
        var publisher = new Mock<IPublisher>();
        var options = new DbContextOptionsBuilder<WriteDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return (new WriteDbContext(options, publisher.Object), publisher);
    }

    private static Game NewGame() => Game.Create(
        "Celeste", Genre.Adventure, Platform.PC, new DateOnly(2018, 1, 25), "Maddy Makes Games", 9.1m, 500);

    [Fact]
    public async Task SaveChanges_ShouldWriteOutboxMessageAndPublishEvent()
    {
        var (context, publisher) = CreateContext();
        var game = NewGame();
        context.Games.Add(game);

        await context.SaveChangesAsync();

        // Persisted to the outbox (at-least-once)...
        var outbox = await context.OutboxMessages.ToListAsync();
        outbox.Should().ContainSingle()
            .Which.Type.Should().Contain(nameof(GameCreatedEvent));

        // ...and dispatched in-process (read-your-writes).
        publisher.Verify(
            p => p.Publish(It.Is<IDomainEvent>(e => e is GameCreatedEvent), It.IsAny<CancellationToken>()),
            Times.Once);

        // Events are drained from the aggregate.
        game.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveChanges_WithNoDomainEvents_ShouldNotPublish()
    {
        var (context, publisher) = CreateContext();
        var game = NewGame();
        context.Games.Add(game);
        await context.SaveChangesAsync();           // drains the create event
        publisher.Invocations.Clear();

        // A change that raises no domain event.
        game.SetCoverImageKey("key_cover.png");      // SetCoverImageKey raises nothing
        await context.SaveChangesAsync();

        publisher.Verify(
            p => p.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
