using FluentValidation.Results;

namespace GameCatalogue.Application.Exceptions;

/// <summary>
/// Exception thrown when one or more FluentValidation rules fail.
/// </summary>
public class ValidationException : Exception
{
    /// <summary>
    /// Gets the validation errors grouped by property name.
    /// </summary>
    public IDictionary<string, string[]> Errors { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class.
    /// </summary>
    public ValidationException()
        : base("One or more validation failures have occurred.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class
    /// from a set of validation failures.
    /// </summary>
    /// <param name="failures">The validation failures.</param>
    public ValidationException(IEnumerable<ValidationFailure> failures)
        : this()
    {
        Errors = failures
            .GroupBy(f => f.PropertyName, f => f.ErrorMessage)
            .ToDictionary(g => g.Key, g => g.ToArray());
    }
}
