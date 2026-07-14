namespace Haven.Application.Mappings.RoomPackages;

/// <summary>
/// Registers flat room package command and entity mappings.
/// </summary>
public sealed class RoomPackageMapping : IRegister
{
    /// <summary>
    /// Registers mappings that do not require database lookups or side effects.
    /// </summary>
    /// <param name="config">The Mapster configuration registry.</param>
    public void Register(TypeAdapterConfig config)
    {
        // Handlers attach current-party context after mapping public command fields.
        config.NewConfig<CreateRoomPackageCommand, RoomPackageCreateRequestModel>()
            .Map(dest => dest.RoomPublicId, src => src.RoomId)
            .Ignore(dest => dest.CurrentParty);

        config.NewConfig<UpdateRoomPackageCommand, RoomPackageUpdateRequestModel>()
            .Map(dest => dest.PackagePublicId, src => src.Id)
            .Map(dest => dest.RoomPublicId, src => src.RoomId)
            .Ignore(dest => dest.CurrentParty);

        // Package creation maps only FE-owned display fields; the service supplies persistence identities and relationships.
        config.NewConfig<RoomPackageCreateRequestModel, UnitPackage>()
            .Map(dest => dest.PackageName, src => src.Name)
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.PublicId)
            .Ignore(dest => dest.PropertyId)
            .Ignore(dest => dest.Property)
            .Ignore(dest => dest.UnitId)
            .Ignore(dest => dest.Unit)
            .Ignore(dest => dest.PackageTypeId)
            .Ignore(dest => dest.PackageCode)
            .Ignore(dest => dest.StatusId)
            .Ignore(dest => dest.Description)
            .Ignore(dest => dest.Items);

        // Partial package updates preserve omitted scalar fields and keep internal persistence fields workflow-owned.
        config.NewConfig<RoomPackageUpdateRequestModel, UnitPackage>()
            .IgnoreNullValues(true)
            .Map(dest => dest.PackageName, src => src.Name)
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.PublicId)
            .Ignore(dest => dest.PropertyId)
            .Ignore(dest => dest.Property)
            .Ignore(dest => dest.UnitId)
            .Ignore(dest => dest.Unit)
            .Ignore(dest => dest.PackageTypeId)
            .Ignore(dest => dest.PackageCode)
            .Ignore(dest => dest.StatusId)
            .Ignore(dest => dest.Description)
            .Ignore(dest => dest.Items);
    }
}
