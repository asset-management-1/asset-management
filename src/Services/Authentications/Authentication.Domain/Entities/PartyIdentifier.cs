namespace Authentication.Domain.Entities;

/// <summary>
/// Represents one identity document or identifier attached to a party.
/// </summary>
public class PartyIdentifier : BaseEntity
{
    /// <summary>
    /// Gets or sets the owning party id.
    /// </summary>
    public long PartyId { get; set; }

    /// <summary>
    /// Gets or sets the identifier type master-data value id.
    /// </summary>
    public long IdentifierTypeId { get; set; }

    /// <summary>
    /// Gets or sets the identifier value.
    /// </summary>
    public string IdentifierValue { get; set; }

    /// <summary>
    /// Gets or sets the full name shown on the document.
    /// </summary>
    public string FullNameOnDocument { get; set; }

    /// <summary>
    /// Gets or sets the date of birth shown on the document.
    /// </summary>
    public DateOnly? DateOfBirthOnDocument { get; set; }

    /// <summary>
    /// Gets or sets the gender shown on the document.
    /// </summary>
    public long? GenderOnDocumentId { get; set; }

    /// <summary>
    /// Gets or sets the current manual review status master-data value id.
    /// </summary>
    public long StatusId { get; set; }

    /// <summary>
    /// Gets or sets the registered address shown on the document.
    /// </summary>
    public string RegisteredAddress { get; set; }

    /// <summary>
    /// Gets or sets the document issue date.
    /// </summary>
    public DateOnly? IssuedDate { get; set; }

    /// <summary>
    /// Gets or sets the document expiration date.
    /// </summary>
    public DateOnly? ExpiredDate { get; set; }

    /// <summary>
    /// Gets or sets the issuing authority.
    /// </summary>
    public string IssuedBy { get; set; }

    /// <summary>
    /// Gets or sets the owning party.
    /// </summary>
    public virtual Party Party { get; set; }

    /// <summary>
    /// Gets or sets the identifier type master-data value.
    /// </summary>
    public virtual MasterDataValue IdentifierType { get; set; }

    /// <summary>
    /// Gets or sets the gender master-data value shown on the document.
    /// </summary>
    public virtual MasterDataValue GenderOnDocument { get; set; }

    /// <summary>
    /// Gets or sets the manual review status master-data value.
    /// </summary>
    public virtual MasterDataValue Status { get; set; }

}
