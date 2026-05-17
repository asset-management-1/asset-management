namespace Authentication.Application.Dtos.Users.ExternalProviders;

/// <summary>
/// Represents a linked external provider row returned by the read model.
/// </summary>
public class LinkedExternalProviderReadModelDto
{
    /// <summary>
    /// External provider name.
    /// </summary>
    public string Provider { get; set; }
}
