namespace Haven.Shared.Dtos.Responses;

/// <summary>
/// Represents the response data received from a third-party Redis service.
/// </summary>
public class RedisThirdPartyResponse
{
    /// <summary>
    /// Gets or sets the status of the operation (e.g., "Success", "Failed").
    /// </summary>
    [JsonProperty("status")]
    public string Status { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the operation was successful.
    /// </summary>
    [JsonProperty("isSuccess")]
    public bool Success { get; set; }

    /// <summary>
    /// The total number of items returned or available in the result set.
    /// </summary>
    [JsonProperty("totalCount")]
    public int TotalCount { get; set; }

    /// <summary>
    /// Gets or sets a message describing the result of the operation.
    /// </summary>
    [JsonProperty("message")]
    public string Message { get; set; }

    /// <summary>
    /// Gets or sets a collection of error details, if any occurred during the operation.
    /// </summary>
    [JsonProperty("errors")]
    public List<string> Errors { get; set; }
}
