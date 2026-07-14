namespace Haven.Infrastructure.Mappings.Rooms;

/// <summary>
/// Registers Mapster mappings for scalar room field mutations.
/// </summary>
public class RoomFieldMutationMapping : IRegister
{
    /// <summary>
    /// Registers partial-update rules for fields that do not need master-data lookup or business guards.
    /// </summary>
    /// <param name="config">The Mapster configuration instance.</param>
    public void Register(TypeAdapterConfig config)
    {
        // Room updates are partial: omitted request values do not erase tracked room data.
        config.NewConfig<RoomFieldUpdateModel, Unit>()
            .IgnoreNullValues(true)
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.PublicId)
            .Ignore(dest => dest.PropertyId)
            .Ignore(dest => dest.Property)
            .Ignore(dest => dest.UnitCode)
            .Ignore(dest => dest.UnitTypeId)
            .Ignore(dest => dest.RentalModeId)
            .Ignore(dest => dest.StatusId)
            .Ignore(dest => dest.IsPublished)
            .Ignore(dest => dest.UnitPackages)
            .Ignore(dest => dest.RentalChargePolicies)
            .Map(dest => dest.UnitName, src => src.Name)
            .Map(dest => dest.BedCount, src => src.TotalBeds);
    }
}
