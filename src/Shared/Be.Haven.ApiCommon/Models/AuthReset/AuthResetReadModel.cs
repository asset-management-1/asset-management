namespace Be.Haven.ApiCommon.Models.AuthReset;

/// <summary>
/// Represents the minimal user auth reset state read by the shared authentication handler.
/// </summary>
public class AuthResetReadModel
{
    /// <summary>
    /// Gets or sets the UTC timestamp after which issued access tokens remain valid.
    /// </summary>
    public DateTime? AuthResetAt { get; set; }
}
