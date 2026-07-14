namespace Haven.Application.Mappings.Properties;

/// <summary>
/// Registers property partial-update mappings for scalar and editable child fields.
/// </summary>
public sealed class PropertyUpdateMapping : IRegister
{
    /// <summary>
    /// Registers update mappings while protecting identity, ownership, and navigation fields.
    /// </summary>
    /// <param name="config">The Mapster configuration instance.</param>
    public void Register(TypeAdapterConfig config)
    {
        // Mapster applies only submitted scalar values; lookup-backed IDs are prepared by the service.
        config.NewConfig<PropertyUpdateRequestModel, Property>()
            .IgnoreNullValues(true)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.PropertyTypeId, src => src.PropertyTypeId.GetValueOrDefault())
            .IgnoreIf((src, _) => !src.PropertyTypeId.HasValue, dest => dest.PropertyTypeId)
            .Map(dest => dest.ProvinceId, src => src.ProvinceId)
            .IgnoreIf((src, _) => !src.ProvinceId.HasValue, dest => dest.ProvinceId)
            .Map(dest => dest.DistrictId, src => src.DistrictId)
            .IgnoreIf((src, _) => !src.DistrictId.HasValue, dest => dest.DistrictId)
            .Map(dest => dest.WardId, src => src.WardId)
            .IgnoreIf((src, _) => !src.WardId.HasValue, dest => dest.WardId)
            .Map(dest => dest.StreetAddress, src => src.StreetAddress)
            .Map(dest => dest.FormattedAddress, src => src.FormattedAddress)
            .Map(dest => dest.Latitude, src => src.Latitude)
            .Map(dest => dest.Longitude, src => src.Longitude)
            .Ignore(dest => dest.PublicId)
            .Ignore(dest => dest.PropertyCode)
            .Ignore(dest => dest.TotalFloors)
            .Ignore(dest => dest.TotalUnits)
            .Ignore(dest => dest.StatusId)
            .Ignore(dest => dest.IsPublished)
            .Ignore(dest => dest.DeleteRequestedAt)
            .Ignore(dest => dest.DeleteScheduledAt)
            .Ignore(dest => dest.DeleteRequestedByPartyId);

        // Child entity mappings preserve ownership, master-data IDs, and collection state for the service.
        config.NewConfig<UpdatePropertyChargePolicyRequestDto, RentalChargePolicy>()
            .IgnoreNullValues(true)
            .Map(dest => dest.ChargeName, src => src.Name)
            .Map(dest => dest.Amount, src => src.Amount)
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.PublicId)
            .Ignore(dest => dest.PropertyId)
            .Ignore(dest => dest.Property)
            .Ignore(dest => dest.UnitId)
            .Ignore(dest => dest.Unit)
            .Ignore(dest => dest.LineTypeId)
            .Ignore(dest => dest.VehicleTypeId)
            .Ignore(dest => dest.IsUsageBased)
            .Ignore(dest => dest.StatusId);

        config.NewConfig<UpdatePropertyPackageRequestDto, UnitPackage>()
            .IgnoreNullValues(true)
            .Map(dest => dest.PackageName, src => src.Name)
            .Map(dest => dest.PriceAdjustment, src => src.PriceAdjustment)
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.PublicId)
            .Ignore(dest => dest.PropertyId)
            .Ignore(dest => dest.Property)
            .Ignore(dest => dest.UnitId)
            .Ignore(dest => dest.Unit)
            .Ignore(dest => dest.PackageTypeId)
            .Ignore(dest => dest.PackageCode)
            .Ignore(dest => dest.Description)
            .Ignore(dest => dest.StatusId)
            .Ignore(dest => dest.Items);
    }
}
