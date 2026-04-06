using System.Collections.Generic;

namespace Xtream.Jelly.Api.Models;

/// <summary>
/// A paginated response wrapper for TV-optimized browsing.
/// Returns a page of items along with total count for navigation.
/// </summary>
/// <typeparam name="T">The type of items in the page.</typeparam>
public class PagedResponse<T>
{
    /// <summary>
    /// Gets or sets the items for the current page.
    /// </summary>
    public IEnumerable<T> Items { get; set; } = [];

    /// <summary>
    /// Gets or sets the total number of items across all pages.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Gets or sets the current page index (0-based).
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Gets or sets the number of items per page.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Gets the total number of pages.
    /// </summary>
    public int TotalPages => PageSize > 0 ? (TotalCount + PageSize - 1) / PageSize : 0;

    /// <summary>
    /// Gets a value indicating whether there is a next page.
    /// </summary>
    public bool HasNextPage => Page < TotalPages - 1;

    /// <summary>
    /// Gets a value indicating whether there is a previous page.
    /// </summary>
    public bool HasPreviousPage => Page > 0;
}
