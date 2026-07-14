namespace Haven.Application.Queries.GetRooms;

/// <summary>
/// Represents a landlord-scoped room ledger query.
/// </summary>
public class GetRoomsQuery : IQuery<ResponseDto<PaginationResponse<IReadOnlyList<RoomListPropertyResponseDto>>>>
{
    /// <summary>
    /// Gets or sets the optional property identifier filter.
    /// </summary>
    public Guid? PropertyId { get; set; }

    /// <summary>
    /// Gets or sets the optional property textbox search term.
    /// </summary>
    public string PropertySearch { get; set; }

    /// <summary>
    /// Gets or sets the optional room textbox search term.
    /// </summary>
    public string Search { get; set; }

    /// <summary>
    /// Gets or sets the optional floor number filter.
    /// </summary>
    public int? FloorNumber { get; set; }

    /// <summary>
    /// Gets or sets the optional room status code filter.
    /// </summary>
    public string RoomStatusCode { get; set; }

    /// <summary>
    /// Gets or sets the optional rental mode code filter.
    /// </summary>
    public string RentalModeCode { get; set; }

    /// <summary>
    /// Gets or sets the optional latest payment status code filter.
    /// </summary>
    public string PaymentStatusCode { get; set; }

    /// <summary>
    /// Gets or sets the requested page number.
    /// </summary>
    public int PageNumber { get; set; } = DEFAULT_PAGE_NUMBER;

    /// <summary>
    /// Gets or sets the requested page size.
    /// </summary>
    public int PageSize { get; set; } = DEFAULT_ROOM_PAGE_SIZE;
}
