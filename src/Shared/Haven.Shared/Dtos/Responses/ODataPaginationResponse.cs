namespace Haven.Shared.Dtos.Responses;

/// <summary>
/// Represents a paginated response for OData queries, extending the base pagination response functionality
/// with additional support for navigation links.
/// </summary>
/// <typeparam name="T">The type of data contained in the response.</typeparam>
public class ODataPaginationResponse<T> : BasePaginationResponse<T>
{
    /// <summary>
    /// Gets or sets the identifier for the next link in pagination.
    /// </summary>
    public string NextLinkId { get; set; }

    /// <summary>
    /// Gets or sets the identifier for the previous link in pagination.
    /// </summary>
    public string PreviousLinkId { get; set; }

    /// <summary>
    /// Default constructor.
    /// Initializes a new instance of the <see cref="PaginationResponse{T}"/> class without populating any parameters.
    /// </summary>
    public ODataPaginationResponse()
    {

    }

    /// <summary>
    /// Constructor that initializes a new instance of the <see cref="PaginationResponse{T}"/> class
    /// with data and a total count of items.
    /// </summary>
    /// <param name="data">The data of type <typeparamref name="T"/>.</param>
    /// <param name="total">The total number of items available.</param>
    public ODataPaginationResponse(T data, int total)
    {
        Items = data;
        Total = total;
    }

    /// <summary>
    /// Constructor that initializes a new instance of the <see cref="PaginationResponse{T}"/> class
    /// with data, the current page number, page size, and total count of items.
    /// </summary>
    /// <param name="data">The data of type <typeparamref name="T"/>.</param>
    /// <param name="pageNumber">The current page number.</param>
    /// <param name="pageSize">The number of items displayed per page.</param>
    /// <param name="total">The total number of items available.</param>
    public ODataPaginationResponse(T data, int pageNumber, int pageSize, int total)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        Items = data;
        Total = total;
    }

    /// <summary>
    /// Constructor that initializes a new instance of the <see cref="PaginationResponse{T}"/> class
    /// with data, the current page number, and page size.
    /// </summary>
    /// <param name="item">The data of type <typeparamref name="T"/>.</param>
    /// <param name="pageNumber">The current page number.</param>
    /// <param name="pageSize">The number of items displayed per page.</param>
    public ODataPaginationResponse(T item, int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        Items = item;
    }

    /// <summary>
    /// Constructor that initializes a new instance of the <see cref="PaginationResponse{T}"/> class
    /// with the current page number and page size.
    /// </summary>
    /// <param name="pageNumber">The current page number.</param>
    /// <param name="pageSize">The number of items displayed per page.</param>
    public ODataPaginationResponse(int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
    
    /// <summary>
    /// Initializes a new instance of <see cref="ODataPaginationResponse{T}"/>.
    /// Used to wrap an OData result (items) together with pagination metadata and link identifiers.
    /// </summary>
    /// <param name="item"> The result payload for the current page (e.g., a collection of entities or a DTO that contains them). </param>
    /// <param name="pageNumber"> The current page number (1-based). </param>
    /// <param name="pageSize"> The number of items per page. </param>
    /// <param name="previousLinkId"> An identifier or token that can be used to request the previous page. Null/empty if there is no previous page. </param>
    /// <param name="nextLinkId"> An identifier or token that can be used to request the next page. Null/empty if there is no next page. </param>
    public ODataPaginationResponse(T item, int pageNumber, int pageSize, string previousLinkId, string nextLinkId)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        Items = item;
        PreviousLinkId = previousLinkId;
        NextLinkId = nextLinkId;
    }
}