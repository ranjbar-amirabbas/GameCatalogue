using FluentValidation;

namespace GameCatalogue.Application.Commands.UpdateGame;

/// <summary>
/// Validates <see cref="UpdateGameCommand"/> instances.
/// </summary>
public class UpdateGameCommandValidator : AbstractValidator<UpdateGameCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateGameCommandValidator"/> class.
    /// </summary>
    public UpdateGameCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

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
