namespace Be.Haven.Shared.Dtos.Responses;

public class BasePaginationResponse<T>
{
    /// <summary>
    /// Gets or sets the current page number for pagination purposes.
    /// This indicates which page of data is being returned.
    /// The default value is 1.
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Gets or sets the number of items to be displayed per page for pagination.
    /// This property helps to define how data should be divided across multiple pages.
    /// The default value is 10.
    /// </summary>
    public int PageSize { get; set; } = 10;
    
    /// <summary>
    /// Gets or sets the total number of AP requests returned.
    /// </summary>
    public int Total { get; set; }
    
    /// <summary>
    /// Gets or sets the list of AP request items.
    /// </summary>
    public T Items { get; set; }
}
