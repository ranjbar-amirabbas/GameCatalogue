using FluentValidation;

namespace GameCatalogue.Application.Commands.CreateGame;

/// <summary>
/// Validates <see cref="CreateGameCommand"/> instances.
/// </summary>
public class CreateGameCommandValidator : AbstractValidator<CreateGameCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateGameCommandValidator"/> class.
    /// </summary>
    public CreateGameCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Developer)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Rating)
            .InclusiveBetween(0, 10);

        RuleFor(x => x.DownloadCount)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.ReleaseDate)
            .NotEqual(default(DateOnly))
            .WithMessage("Release date is required.");
    }
}
