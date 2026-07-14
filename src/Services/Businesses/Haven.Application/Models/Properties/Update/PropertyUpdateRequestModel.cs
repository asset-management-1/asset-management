namespace Haven.Application.Models.Properties.Update;

/// <summary>
/// Represents a party-scoped property update request enriched for the infrastructure service.
/// </summary>
public class PropertyUpdateRequestModel
{
    /// <summary>
    /// Gets or sets the property public identifier from the route.
    /// </summary>
    public Guid PropertyPublicId { get; set; }

    /// <summary>
    /// Gets or sets the current landlord party context.
    /// </summary>
    public CurrentPartyContextModel CurrentParty { get; set; }

    /// <summary>
    /// Gets or sets the property name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the property type code.
    /// </summary>
    public string PropertyTypeCode { get; set; }

    /// <summary>
    /// Gets or sets the resolved property-type identifier when that combobox value is submitted.
    /// </summary>
    public long? PropertyTypeId { get; set; }

    /// <summary>
    /// Gets or sets the province code.
    /// </summary>
    public string ProvinceCode { get; set; }

    /// <summary>
    /// Gets or sets the resolved province identifier when that address value is submitted.
    /// </summary>
    public long? ProvinceId { get; set; }

    /// <summary>
    /// Gets or sets the district code.
    /// </summary>
    public string DistrictCode { get; set; }

    /// <summary>
    /// Gets or sets the resolved district identifier when that address value is submitted.
    /// </summary>
    public long? DistrictId { get; set; }

    /// <summary>
    /// Gets or sets the ward code.
    /// </summary>
    public string WardCode { get; set; }

    /// <summary>
    /// Gets or sets the resolved ward identifier when that address value is submitted.
    /// </summary>
    public long? WardId { get; set; }

    /// <summary>
    /// Gets or sets the street address.
    /// </summary>
    public string StreetAddress { get; set; }

    /// <summary>
    /// Gets or sets the formatted address.
    /// </summary>
    public string FormattedAddress { get; set; }

    /// <summary>
    /// Gets or sets latitude.
    /// </summary>
    public decimal? Latitude { get; set; }

    /// <summary>
    /// Gets or sets longitude.
    /// </summary>
    public decimal? Longitude { get; set; }

    /// <summary>
    /// Gets or sets the edited property structure when the structure section is submitted.
    /// </summary>
    public UpdatePropertyStructureRequestDto Structure { get; set; }

    /// <summary>
    /// Gets or sets the edited property-level charge policies when that section is submitted.
    /// </summary>
    public IReadOnlyList<UpdatePropertyChargePolicyRequestDto> ChargePolicies { get; set; }

    /// <summary>
    /// Gets or sets the edited common package templates when that section is submitted.
    /// </summary>
    public IReadOnlyList<UpdatePropertyPackageRequestDto> Packages { get; set; }

    /// <summary>
    /// Gets or sets resolved location values for the edited address.
    /// </summary>
    public PropertyLocationContextModel Locations { get; set; }
}
