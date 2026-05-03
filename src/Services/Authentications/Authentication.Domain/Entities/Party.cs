namespace Authentication.Domain.Entities;

/// <summary>
/// Represents a business or real-world entity (person or organization)
/// that can be associated with user accounts.
/// </summary>
public class Party
{
    /// <summary>
    /// Internal primary key.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Public identifier used for API/UI exposure.
    /// </summary>
    public Guid PublicId { get; set; }

    /// <summary>
    /// Type of party (e.g., Individual, Organization).
    /// </summary>
    public long PartyTypeId { get; set; }

    /// <summary>
    /// Display name used in UI.
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// Legal/official name of the party.
    /// </summary>
    public string LegalName { get; set; }

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
    /// Record creation timestamp (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// User who created the record.
    /// </summary>
    public long? CreatedBy { get; set; }

    /// <summary>
    /// Last update timestamp (UTC).
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// User who last updated the record.
    /// </summary>
    public long? UpdatedBy { get; set; }

    /// <summary>
    /// Soft delete flag.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Navigation to party type master data.
    /// </summary>
    public virtual MasterDataValue PartyType { get; set; }

    /// <summary>
    /// Navigation to status master data.
    /// </summary>
    public virtual MasterDataValue Status { get; set; }

    /// <summary>
    /// Related user accounts linked to this party.
    /// </summary>
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}