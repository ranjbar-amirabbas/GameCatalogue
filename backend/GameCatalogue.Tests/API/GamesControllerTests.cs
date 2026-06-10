using FluentAssertions;
using GameCatalogue.API.Controllers.v1;
using GameCatalogue.API.Models;
using GameCatalogue.Application.Commands.CreateGame;
using GameCatalogue.Application.Commands.DeleteGame;
using GameCatalogue.Application.Commands.UpdateGame;
using GameCatalogue.Application.DTOs;
using GameCatalogue.Application.Queries.GetGameById;
using GameCatalogue.Application.Queries.GetGames;
using GameCatalogue.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GameCatalogue.Tests.API;

public class GamesControllerTests
{
    private readonly Mock<IMediator> _mediator = new();

    private GamesController CreateController() => new(_mediator.Object);

    private static GameDto SampleGame(Guid id) => new(
        id, "Elden Ring", Genre.RPG, Platform.PC, new DateOnly(2022, 2, 25),
        "FromSoftware", 9.7m, 5_000_000, "url", DateTime.UtcNow, DateTime.UtcNow);

    [Fact]
    public async Task GetAll_ShouldReturn200WithPagedResult()
    {
        var paged = new PagedResult<GameListDto>(
            new List<GameListDto>(), 0, 1, 10);
        _mediator.Setup(m => m.Send(It.IsAny<GetGamesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paged);

        var result = await CreateController().GetAll(ct: CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.StatusCode.Should().Be(200);
        ok.Value.Should().Be(paged);
    }

    [Fact]
    public async Task GetById_WhenExists_ShouldReturn200()
    {
        var id = Guid.NewGuid();
        _mediator.Setup(m => m.Send(It.IsAny<GetGameByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SampleGame(id));

        var result = await CreateController().GetById(id, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GetById_WhenNotExists_ShouldReturn404()
    {
        _mediator.Setup(m => m.Send(It.IsAny<GetGameByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GameDto?)null);

        var result = await CreateController().GetById(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Create_WithValidCommand_ShouldReturn201()
    {
        var id = Guid.NewGuid();
        _mediator.Setup(m => m.Send(It.IsAny<CreateGameCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(id);

        var form = new CreateGameForm
        {
            Title = "New Game",
            Genre = Genre.Action,
            Platform = Platform.PC,
            ReleaseDate = new DateOnly(2023, 1, 1),
            Developer = "Dev",
            Rating = 8m,
            DownloadCount = 0,
            File = null
        };

        var result = await CreateController().Create(form, CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.StatusCode.Should().Be(201);
        created.Value.Should().Be(id);
    }

    [Fact]
    public async Task Update_WithValidCommand_ShouldReturn204()
    {
        _mediator.Setup(m => m.Send(It.IsAny<UpdateGameCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var id = Guid.NewGuid();
        var command = new UpdateGameCommand(
            id, "Updated", Genre.Action, Platform.PC, new DateOnly(2023, 1, 1), "Dev", 8m, 0);

        var result = await CreateController().Update(id, command, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_WhenExists_ShouldReturn204()
    {
        _mediator.Setup(m => m.Send(It.IsAny<DeleteGameCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await CreateController().Delete(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }
}
