namespace Haven.Application.Commands.CreateProperty;

/// <summary>
/// Represents a request to create a landlord property/building with generated units.
/// </summary>
public class CreatePropertyCommand : ICommand<ResponseDto<CreatedPropertyResponseDto>>
{
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
    /// Gets or sets the generated structure setup.
    /// </summary>
    public CreatePropertyStructureRequestDto StructureSetup { get; set; }

    /// <summary>
    /// Gets or sets optional property-level charge policies.
    /// </summary>
    public IReadOnlyList<CreatePropertyChargePolicyRequestDto> ChargePolicies { get; set; } = [];

    /// <summary>
    /// Gets or sets optional service/furniture packages copied to each generated room.
    /// </summary>
    public IReadOnlyList<CreatePropertyPackageRequestDto> Packages { get; set; } = [];
}
