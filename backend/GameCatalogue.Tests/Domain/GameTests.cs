using FluentAssertions;
using GameCatalogue.Domain.Entities;
using GameCatalogue.Domain.Enums;
using GameCatalogue.Domain.Events;
using GameCatalogue.Domain.Exceptions;

namespace GameCatalogue.Tests.Domain;

public class GameTests
{
    private static Game CreateValidGame() => Game.Create(
        title: "The Witcher 3",
        genre: Genre.RPG,
        platform: Platform.PC,
        releaseDate: new DateOnly(2015, 5, 19),
        developer: "CD Projekt Red",
        rating: 9.8m,
        downloadCount: 1_000_000);

    [Fact]
    public void Create_WithValidInputs_ShouldCreateGame()
    {
        var game = CreateValidGame();

        game.Should().NotBeNull();
        game.Id.Should().NotBeEmpty();
        game.Title.Should().Be("The Witcher 3");
        game.Genre.Should().Be(Genre.RPG);
        game.Platform.Should().Be(Platform.PC);
        game.Developer.Should().Be("CD Projekt Red");
        game.Rating.Should().Be(9.8m);
        game.DownloadCount.Should().Be(1_000_000);
        game.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        game.UpdatedAt.Should().Be(game.CreatedAt);
    }

    [Fact]
    public void Create_WithEmptyTitle_ShouldThrowDomainException()
    {
        var act = () => Game.Create(
            title: "   ",
            genre: Genre.Action,
            platform: Platform.PC,
            releaseDate: new DateOnly(2020, 1, 1),
            developer: "Studio",
            rating: 5m,
            downloadCount: 0);

        act.Should().Throw<DomainException>().WithMessage("*Title*");
    }

    [Fact]
    public void Create_WithRatingAbove10_ShouldThrowDomainException()
    {
        var act = () => Game.Create(
            title: "Some Game",
            genre: Genre.Action,
            platform: Platform.PC,
            releaseDate: new DateOnly(2020, 1, 1),
            developer: "Studio",
            rating: 10.5m,
            downloadCount: 0);

        act.Should().Throw<DomainException>().WithMessage("*Rating*");
    }

    [Fact]
    public void Create_ShouldRaiseGameCreatedEvent()
    {
        var game = CreateValidGame();

        game.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<GameCreatedEvent>()
            .Which.GameId.Should().Be(game.Id);
    }

    [Fact]
    public void Update_ShouldRaiseGameUpdatedEvent()
    {
        var game = CreateValidGame();
        game.ClearDomainEvents();

        game.Update(
            title: "The Witcher 3: Wild Hunt",
            genre: Genre.RPG,
            platform: Platform.PlayStation,
            releaseDate: new DateOnly(2015, 5, 19),
            developer: "CD Projekt Red",
            rating: 10m,
            downloadCount: 2_000_000);

        game.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<GameUpdatedEvent>()
            .Which.Title.Should().Be("The Witcher 3: Wild Hunt");
    }

    [Fact]
    public void Update_ShouldSetUpdatedAt()
    {
        var game = CreateValidGame();
        var originalUpdatedAt = game.UpdatedAt;
        Thread.Sleep(10);

        game.Update(
            title: "Updated Title",
            genre: Genre.Action,
            platform: Platform.Xbox,
            releaseDate: new DateOnly(2018, 3, 1),
            developer: "Studio",
            rating: 7.5m,
            downloadCount: 500);

        game.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }
}
