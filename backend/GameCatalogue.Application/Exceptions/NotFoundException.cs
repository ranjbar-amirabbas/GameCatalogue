namespace GameCatalogue.Application.Exceptions;

/// <summary>
/// Exception thrown when a requested entity cannot be found.
/// </summary>
public class NotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class.
    /// </summary>
    /// <param name="name">The name of the entity type.</param>
    /// <param name="key">The key that was searched for.</param>
    public NotFoundException(string name, object key)
        : base($"{name} with key {key} was not found.")
    {
    }
}
