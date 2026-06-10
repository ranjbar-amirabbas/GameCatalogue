using FluentAssertions;
using GameCatalogue.Application.DTOs;
using GameCatalogue.Application.Interfaces.Cache;
using GameCatalogue.Application.Interfaces.Storage;
using GameCatalogue.Application.Queries.GetGameById;
using GameCatalogue.Domain.Entities;
using GameCatalogue.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace GameCatalogue.Tests.Application;

public class GetGameByIdQueryHandlerTests
{
    private readonly Mock<ICacheService> _cache = new();
    private readonly Mock<ICoverImageUrlResolver> _urlResolver = new();
    private readonly Mock<ILogger<GetGameByIdQueryHandler>> _logger = new();

    public GetGameByIdQueryHandlerTests()
    {
        // Identity resolver: return the key unchanged so value equality holds in tests.
        _urlResolver.Setup(r => r.Resolve(It.IsAny<string?>(), It.IsAny<int?>()))
            .Returns((string? key, int? _) => key ?? string.Empty);
    }

    private static Game SeedGame(TestReadDbContext context)
    {
        var game = Game.Create(
            "Stardew Valley", Genre.Simulation, Platform.PC,
            new DateOnly(2016, 2, 26), "ConcernedApe", 9.5m, 3_000_000);
        context.GamesSet.Add(game);
        context.SaveChanges();
        return game;
    }

    [Fact]
    public async Task Handle_WhenCacheHit_ShouldReturnFromCache()
    {
        using var context = TestReadDbContext.CreateInMemory();
        var id = Guid.NewGuid();
        var cached = new GameDto(id, "Cached Game", Genre.RPG, Platform.PC,
            new DateOnly(2020, 1, 1), "Dev", 7m, 100, "url", DateTime.UtcNow, DateTime.UtcNow);

        _cache.Setup(c => c.GetAsync<GameDto>($"game:{id}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cached);

        var handler = new GetGameByIdQueryHandler(context, _cache.Object, _urlResolver.Object, _logger.Object);

        var result = await handler.Handle(new GetGameByIdQuery(id), CancellationToken.None);

        result.Should().Be(cached);
    }

    [Fact]
    public async Task Handle_WhenCacheMiss_ShouldQueryDatabase()
    {
        using var context = TestReadDbContext.CreateInMemory();
        var game = SeedGame(context);

        _cache.Setup(c => c.GetAsync<GameDto>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GameDto?)null);

        var handler = new GetGameByIdQueryHandler(context, _cache.Object, _urlResolver.Object, _logger.Object);

        var result = await handler.Handle(new GetGameByIdQuery(game.Id), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(game.Id);
        result.Title.Should().Be("Stardew Valley");
    }

    [Fact]
    public async Task Handle_WhenCacheMiss_ShouldPopulateCache()
    {
        using var context = TestReadDbContext.CreateInMemory();
        var game = SeedGame(context);

        _cache.Setup(c => c.GetAsync<GameDto>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GameDto?)null);

        var handler = new GetGameByIdQueryHandler(context, _cache.Object, _urlResolver.Object, _logger.Object);

        await handler.Handle(new GetGameByIdQuery(game.Id), CancellationToken.None);

        _cache.Verify(
            c => c.SetAsync(
                $"game:{game.Id}",
                It.IsAny<GameDto>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenGameNotFound_ShouldReturnNull()
    {
        using var context = TestReadDbContext.CreateInMemory();

        _cache.Setup(c => c.GetAsync<GameDto>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GameDto?)null);

        var handler = new GetGameByIdQueryHandler(context, _cache.Object, _urlResolver.Object, _logger.Object);

        var result = await handler.Handle(new GetGameByIdQuery(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
    }
}
