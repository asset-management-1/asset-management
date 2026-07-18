namespace Haven.Domain.Entities;

/// <summary>
/// Represents a business party profile in core.Parties.
/// </summary>
public class Party : BaseEntity
{
    /// <summary>
    /// Gets or sets the party type master-data identifier.
    /// </summary>
    public long PartyTypeId { get; set; }

    /// <summary>
    /// Gets or sets the party display name.
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the primary business phone.
    /// </summary>
    public string PrimaryPhone { get; set; }

    /// <summary>
    /// Gets or sets the primary business email.
    /// </summary>
    public string PrimaryEmail { get; set; }

    /// <summary>
    /// Gets or sets the party status master-data identifier.
    /// </summary>
    public long StatusId { get; set; }

    /// <summary>
    /// Gets the User relationships that expose this Party as an available context.
    /// </summary>
    public ICollection<UserParty> UserParties { get; } = new List<UserParty>();

    /// <summary>
    /// Gets the active or historical occupancies linked to this party.
    /// </summary>
    public ICollection<Occupancy> Occupancies { get; } = new List<Occupancy>();
}
