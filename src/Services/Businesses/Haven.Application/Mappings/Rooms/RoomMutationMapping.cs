namespace Haven.Application.Mappings.Rooms;

/// <summary>
/// Registers partial mappings for editable room child entities.
/// </summary>
public sealed class RoomMutationMapping : IRegister
{
    /// <summary>
    /// Registers nullable child-field mappings while preserving resolved ownership fields.
    /// </summary>
    /// <param name="config">The Mapster configuration instance.</param>
    public void Register(TypeAdapterConfig config)
    {
        // Mapster updates only submitted policy fields; the service resolves code-backed IDs.
        config.NewConfig<UpdateRoomChargePolicyRequestDto, RentalChargePolicy>()
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

        // Package identity and item collections remain under repository-owned synchronization.
        config.NewConfig<UpdateRoomPackageRequestDto, UnitPackage>()
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
