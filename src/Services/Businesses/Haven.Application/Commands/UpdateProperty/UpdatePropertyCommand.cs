namespace Haven.Application.Commands.UpdateProperty;

/// <summary>
/// Represents a request to update a landlord property/building edit form.
/// </summary>
public class UpdatePropertyCommand : ICommand<ResponseDto<PropertyDetailResponseDto>>
{
    /// <summary>
    /// Gets or sets the frontend-safe property identifier from the request body.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the property name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the property type code.
    /// </summary>
    public string PropertyTypeCode { get; set; }

    /// <summary>
    /// Gets or sets the province code.
    /// </summary>
    public string ProvinceCode { get; set; }

    /// <summary>
    /// Gets or sets the district code.
    /// </summary>
    public string DistrictCode { get; set; }

    /// <summary>
    /// Gets or sets the ward code.
    /// </summary>
    public string WardCode { get; set; }

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
    /// Gets or sets the full edited room structure when the structure section is submitted.
    /// </summary>
    public UpdatePropertyStructureRequestDto Structure { get; set; }

    /// <summary>
    /// Gets or sets edited property-level charge policies when the common pricing section is submitted.
    /// </summary>
    public IReadOnlyList<UpdatePropertyChargePolicyRequestDto> ChargePolicies { get; set; }

    /// <summary>
    /// Gets or sets edited common package templates when the package section is submitted.
    /// </summary>
    public IReadOnlyList<UpdatePropertyPackageRequestDto> Packages { get; set; }
}
