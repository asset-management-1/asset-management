namespace Haven.Application.Models.Vehicles.Create;

/// <summary>
/// Represents the party-scoped vehicle creation input used by the service layer.
/// </summary>
public class VehicleCreateRequestModel
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
    /// Gets or sets the payer tenant identifier.
    /// </summary>
    public Guid PayerTenantId { get; set; }

    /// <summary>
    /// Gets or sets the vehicle-type code.
    /// </summary>
    public string VehicleTypeCode { get; set; }

    /// <summary>
    /// Gets or sets the vehicle display name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the license plate.
    /// </summary>
    public string LicensePlate { get; set; }

    /// <summary>
    /// Gets or sets the registration front image.
    /// </summary>
    public IFormFile RegistrationFrontImage { get; set; }

    /// <summary>
    /// Gets or sets the registration side image.
    /// </summary>
    public IFormFile RegistrationSideImage { get; set; }

    /// <summary>
    /// Gets or sets the vehicle front image.
    /// </summary>
    public IFormFile VehicleFrontImage { get; set; }

    /// <summary>
    /// Gets or sets the vehicle side image.
    /// </summary>
    public IFormFile VehicleSideImage { get; set; }
}
