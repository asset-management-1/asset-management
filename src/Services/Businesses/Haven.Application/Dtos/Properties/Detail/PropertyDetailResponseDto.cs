namespace Haven.Application.Dtos.Properties.Detail;

/// <summary>
/// Represents the landlord building/detail response.
/// </summary>
public class PropertyDetailResponseDto
{
    /// <summary>
    /// Gets or sets the basic property information used by detail and edit screens.
    /// </summary>
    public PropertyDetailBasicInfoResponseDto BasicInfo { get; set; }

    /// <summary>
    /// Gets or sets the full editable address information.
    /// </summary>
    public PropertyAddressResponseDto Address { get; set; }

    /// <summary>
    /// Gets or sets the structure section derived from property units.
    /// </summary>
    public PropertyDetailStructureResponseDto Structure { get; set; }

    /// <summary>
    /// Gets or sets property-level charge policies used to refill the fee setup form.
    /// </summary>
    public IReadOnlyList<PropertyChargePolicyResponseDto> ChargePolicies { get; set; } = [];

    /// <summary>
    /// Gets or sets package templates configured for rooms under the property.
    /// </summary>
    public IReadOnlyList<PropertyPackageTemplateResponseDto> PackageTemplates { get; set; } = [];

    /// <summary>
    /// Gets or sets management counters for detail cards.
    /// </summary>
    public PropertyManagementSummaryResponseDto ManagementSummary { get; set; }
}

