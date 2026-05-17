namespace Be.Haven.Shared.Dtos.Responses;

/// <summary>
/// Represents a generic response object for paginated data. 
/// This class is designed to facilitate pagination by providing metadata 
/// about the total number of items, pages, and the current page's data of type <typeparamref name="T"/>.
/// </summary>
public class PaginationResponse<T> : BasePaginationResponse<T>
{
    /// <summary>
    /// Gets the total number of pages required for pagination based on the total count of items and the page size.
    /// If the page size is zero, it defaults to 1 to prevent division by zero.
    /// </summary>
    public int TotalPages => (Total - 1) / (PageSize == 0 ? 1 : PageSize) + 1;

    /// <summary>
    /// Default constructor.
    /// Initializes a new instance of the <see cref="PaginationResponse{T}"/> class without populating any parameters.
    /// </summary>
    public PaginationResponse()
    {
    }

    /// <summary>
    /// Constructor that initializes a new instance of the <see cref="PaginationResponse{T}"/> class
    /// with data and a total count of items.
    /// </summary>
    /// <param name="data">The data of type <typeparamref name="T"/>.</param>
    /// <param name="total">The total number of items available.</param>
    public PaginationResponse(T data, int total)
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
    public PaginationResponse(T data, int pageNumber, int pageSize, int total)
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
    public PaginationResponse(T item, int pageNumber, int pageSize)
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
    public PaginationResponse(int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}
