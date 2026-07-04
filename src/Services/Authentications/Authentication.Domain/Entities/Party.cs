namespace Authentication.Domain.Entities;

/// <summary>
/// Represents a business or real-world entity (person or organization)
/// that can be associated with user accounts.
/// </summary>
public class Party : BaseEntity
{
    /// <summary>
    /// Type of party (e.g., Individual, Organization).
    /// </summary>
    public long PartyTypeId { get; set; }

    /// <summary>
    /// Display name used in UI.
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// Primary contact phone number.
    /// </summary>
    public string PrimaryPhone { get; set; }

    /// <summary>
    /// Primary contact email address.
    /// </summary>
    public string PrimaryEmail { get; set; }

    /// <summary>
    /// Current status of the party (e.g., Active, Inactive).
    /// </summary>
    public long StatusId { get; set; }

    /// <summary>
    /// Navigation to party type master data.
    /// </summary>
    public virtual MasterDataValue PartyType { get; set; }

    /// <summary>
    /// Navigation to status master data.
    /// </summary>
    public virtual MasterDataValue Status { get; set; }

    /// <summary>
    /// Related user-party context mappings linked to this party.
    /// </summary>
    public virtual ICollection<UserParty> UserParties { get; set; } = new List<UserParty>();

}
