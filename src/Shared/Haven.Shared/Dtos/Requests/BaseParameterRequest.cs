namespace Haven.Shared.Dtos.Requests;

/// <summary>
/// Represents a common request model that includes pagination
/// and sorting information used for querying data collections.
/// </summary>
public class BaseParameterRequest
{
    /// <summary>
    /// Gets or sets the current page number requested.
    /// The default value is 1.
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Gets or sets the number of items to be returned per page.
    /// The default value is 10.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Gets or sets the string defining the sorting rule.
    /// Format example: "Name Asc" or "CreatedDate Desc".
    /// </summary>
    public string OrderBy { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseParameterRequest"/> class
    /// with the default pagination values.
    /// </summary>
    public BaseParameterRequest()
    {
        PageNumber = 1;
        PageSize = 10;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseParameterRequest"/> class
    /// with custom pagination values.
    /// </summary>
    /// <param name="pageNumber">The page number to request.</param>
    /// <param name="pageSize">The number of records per page.</param>
    public BaseParameterRequest(int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    /// <summary>
    /// Gets or sets the keyword entered by the client.
    /// This value will be applied across all configured search fields.
    /// </summary>
    public string SearchTerm { get; set; }

    /// <summary>
    /// Sets the list of search field configurations.
    /// These configurations define which fields can be searched
    /// and the data type of each field (e.g., string, number).
    /// </summary>
    /// <param name="searchProps">A list of field definitions used for search filtering.</param>
    public void SetSearchProps(List<SearchFieldConfiguration> searchProps) =>
        SearchProps = searchProps;

    /// <summary>
    /// Gets the list of configured search fields.
    /// Each field determines how the search term should be applied.
    /// </summary>
    /// <returns>A list of <see cref="SearchFieldConfiguration"/> items.</returns>
    public List<SearchFieldConfiguration> GetSearchProps() => SearchProps;

    /// <summary>
    /// Stores the internal list of search field configurations.
    /// This collection is used by the search builder to generate
    /// the appropriate OData filter expressions.
    /// </summary>
    private List<SearchFieldConfiguration> SearchProps { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to bypass the cache.
    /// </summary>
    public bool BypassCache { get; set; }

    /// <summary>
    /// Gets or sets the absolute expiration time for the cache.
    /// </summary>
    public TimeSpan? AbsoluteExpiration { get; set; }
}
