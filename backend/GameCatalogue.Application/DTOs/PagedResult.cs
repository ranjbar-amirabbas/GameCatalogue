namespace GameCatalogue.Application.DTOs;

/// <summary>
/// A page of results together with pagination metadata.
/// </summary>
/// <typeparam name="T">The item type.</typeparam>
public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize)
{
    /// <summary>Gets the total number of pages.</summary>
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>Gets a value indicating whether a next page exists.</summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>Gets a value indicating whether a previous page exists.</summary>
    public bool HasPreviousPage => Page > 1;
}
