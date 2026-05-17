namespace Be.Haven.Core.Interfaces.Queries;

/// <summary>
/// Represents the base query parameters used for paginated and searchable requests.
/// </summary>
public interface IQueryBaseRequest
{
    /// <summary>
    /// Gets or sets the current page number for pagination.
    /// The value should be greater than or equal to 1.
    /// </summary>
    int PageNumber { get; set; }

    /// <summary>
    /// Gets or sets the number of items to be returned per page.
    /// The value should be greater than 0.
    /// </summary>
    int PageSize { get; set; }

    /// <summary>
    /// Gets or sets the search keyword used to filter the result set.
    /// This value can be null or empty if no filtering is required.
    /// </summary>
    string SearchTerm { get; set; }

    /// <summary>
    /// Gets or sets the sorting expression for the result set.
    /// For example: "CreatedOn desc" or "Title asc".
    /// </summary>
    string OrderBy { get; set; }
}
