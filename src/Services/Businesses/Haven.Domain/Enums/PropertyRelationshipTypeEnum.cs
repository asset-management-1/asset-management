namespace Haven.Domain.Enums;

/// <summary>
/// Represents supported property-party relationship types used by landlord-facing property reads.
/// </summary>
public enum PropertyRelationshipTypeEnum
{
    /// <summary>
    /// Owner relationship.
    /// </summary>
    Owner = 1,

    /// <summary>
    /// Landlord relationship.
    /// </summary>
    Landlord = 2,

    /// <summary>
    /// Manager relationship.
    /// </summary>
    Manager = 3,

    /// <summary>
    /// Operator relationship.
    /// </summary>
    Operator = 4,

    /// <summary>
    /// Staff relationship.
    /// </summary>
    Staff = 5
}
