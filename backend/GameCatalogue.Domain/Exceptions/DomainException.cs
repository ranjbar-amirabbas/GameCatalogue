namespace GameCatalogue.Domain.Exceptions;

/// <summary>
/// Exception thrown when a domain invariant is violated.
/// </summary>
public class DomainException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DomainException"/> class.
    /// </summary>
    /// <param name="message">The message describing the violated invariant.</param>
    public DomainException(string message) : base(message)
    {
    }
}
