namespace Haven.Domain.Entities;

/// <summary>
/// Represents the relationship between a party and a managed property.
/// </summary>
public class PropertyParty : BaseEntity
{
    /// <summary>
    /// Gets or sets the internal property identifier.
    /// </summary>
    public long PropertyId { get; set; }

    /// <summary>
    /// Gets or sets the parent property navigation.
    /// </summary>
    public Property Property { get; set; }

    /// <summary>
    /// Gets or sets the internal party identifier.
    /// </summary>
    public long PartyId { get; set; }

    /// <summary>
    /// Gets or sets the property relationship type master-data identifier.
    /// </summary>
    public long RelationshipTypeId { get; set; }

    /// <summary>
    /// Gets or sets the relationship start date.
    /// </summary>
    public DateOnly? StartDate { get; set; }

    /// <summary>
    /// Gets or sets the relationship end date.
    /// </summary>
    public DateOnly? EndDate { get; set; }

    /// <summary>
    /// Gets or sets the optional relationship note.
    /// </summary>
    public string Note { get; set; }
}
