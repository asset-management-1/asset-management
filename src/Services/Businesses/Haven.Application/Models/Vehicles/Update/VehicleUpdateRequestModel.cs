namespace Haven.Application.Models.Vehicles.Update;

/// <summary>
/// Represents the party-scoped partial vehicle update input used by the service layer.
/// </summary>
public class VehicleUpdateRequestModel
{
    /// <summary>
    /// Gets or sets the current party context.
    /// </summary>
    public CurrentPartyContextModel CurrentParty { get; set; }

    /// <summary>
    /// Gets or sets the owning room identifier.
    /// </summary>
    public Guid RoomId { get; set; }

    /// <summary>
    /// Gets or sets the vehicle identifier.
    /// </summary>
    public Guid VehicleId { get; set; }

    /// <summary>
    /// Gets or sets the replacement payer tenant identifier.
    /// </summary>
    public Guid? PayerTenantId { get; set; }

    /// <summary>
    /// Gets or sets the replacement vehicle-type code.
    /// </summary>
    public string VehicleTypeCode { get; set; }

    /// <summary>
    /// Gets or sets the replacement vehicle name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the replacement license plate.
    /// </summary>
    public string LicensePlate { get; set; }

    /// <summary>
    /// Gets or sets the replacement registration front image.
    /// </summary>
    public IFormFile RegistrationFrontImage { get; set; }

    /// <summary>
    /// Gets or sets the replacement registration side image.
    /// </summary>
    public IFormFile RegistrationSideImage { get; set; }

    /// <summary>
    /// Gets or sets the replacement vehicle front image.
    /// </summary>
    public IFormFile VehicleFrontImage { get; set; }

    /// <summary>
    /// Gets or sets the replacement vehicle side image.
    /// </summary>
    public IFormFile VehicleSideImage { get; set; }
}
