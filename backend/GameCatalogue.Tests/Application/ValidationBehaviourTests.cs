using FluentAssertions;
using FluentValidation;
using GameCatalogue.Application.Behaviours;
using GameCatalogue.Application.Commands.CreateGame;
using GameCatalogue.Domain.Enums;
using MediatR;
using ValidationException = GameCatalogue.Application.Exceptions.ValidationException;

namespace GameCatalogue.Tests.Application;

public class ValidationBehaviourTests
{
    private static CreateGameCommand ValidCommand() => new(
        "Title", Genre.Action, Platform.PC, new DateOnly(2020, 1, 1), "Dev", 5m, 0);

    private sealed class AlwaysFailsValidator : AbstractValidator<CreateGameCommand>
    {
        public AlwaysFailsValidator() => RuleFor(x => x.Title).Must(_ => false).WithMessage("forced failure");
    }

    [Fact]
    public async Task Handle_WhenNoValidators_ShouldCallNext()
    {
        var behaviour = new ValidationBehaviour<CreateGameCommand, Guid>(
            Enumerable.Empty<IValidator<CreateGameCommand>>());

        var expected = Guid.NewGuid();
        var nextCalled = false;
        RequestHandlerDelegate<Guid> next = () =>
        {
            nextCalled = true;
            return Task.FromResult(expected);
        };

        var result = await behaviour.Handle(ValidCommand(), next, CancellationToken.None);

        nextCalled.Should().BeTrue();
        result.Should().Be(expected);
    }

    [Fact]
    public async Task Handle_WhenValidationPasses_ShouldCallNext()
    {
        var behaviour = new ValidationBehaviour<CreateGameCommand, Guid>(
            new IValidator<CreateGameCommand>[] { new CreateGameCommandValidator() });

        var nextCalled = false;
        RequestHandlerDelegate<Guid> next = () =>
        {
            nextCalled = true;
            return Task.FromResult(Guid.NewGuid());
        };

        await behaviour.Handle(ValidCommand(), next, CancellationToken.None);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenValidationFails_ShouldThrowValidationException()
    {
        var behaviour = new ValidationBehaviour<CreateGameCommand, Guid>(
            new IValidator<CreateGameCommand>[] { new AlwaysFailsValidator() });

        var nextCalled = false;
        RequestHandlerDelegate<Guid> next = () =>
        {
            nextCalled = true;
            return Task.FromResult(Guid.NewGuid());
        };

        var act = () => behaviour.Handle(ValidCommand(), next, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        nextCalled.Should().BeFalse();
    }
}
