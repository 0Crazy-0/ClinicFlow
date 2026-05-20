namespace ClinicFlow.Application.Common.Models;

/// <summary>
/// Represents a paginated list of items with page metadata.
/// </summary>
/// <typeparam name="T">The type of items in the list.</typeparam>
public class PaginatedList<T>(
    IReadOnlyCollection<T> items,
    int totalCount,
    int pageNumber,
    int pageSize
)
{
    public IReadOnlyCollection<T> Items { get; } = items;

    public int TotalCount { get; } = totalCount;

    public int PageNumber { get; } = pageNumber;

    public int TotalPages { get; } = (int)Math.Ceiling(totalCount / (double)pageSize);

    public bool HasPreviousPage => PageNumber > 1;

    public bool HasNextPage => PageNumber < TotalPages;
}
