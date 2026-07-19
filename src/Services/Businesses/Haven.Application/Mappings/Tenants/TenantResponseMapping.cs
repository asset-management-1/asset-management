namespace Haven.Application.Mappings.Tenants;

/// <summary>
/// Registers tenant read-model and join-preview response mappings.
/// </summary>
public sealed class TenantResponseMapping : IRegister
{
    /// <summary>
    /// Registers flat tenant, room, property, and join-preview projections.
    /// </summary>
    /// <param name="config">The Mapster configuration instance.</param>
    public void Register(TypeAdapterConfig config)
    {
        // Tenant read rows are flat Dapper projections, so Mapster can shape compact response DTOs.
        config.NewConfig<TenantRowModel, TenantListItemResponseDto>()
            .Map(dest => dest.Id, src => src.OccupancyPublicId)
            .Map(dest => dest.TenantId, src => src.TenantPublicId)
            .Map(dest => dest.Tenant, src => src.TenantName)
            .Map(dest => dest.Property, src => src.Adapt<TenantPropertySummaryResponseDto>())
            .Map(dest => dest.Room, src => src.Adapt<TenantRoomSummaryResponseDto>());

        config.NewConfig<TenantRowModel, TenantDetailResponseDto>()
            .Inherits<TenantRowModel, TenantListItemResponseDto>();

        config.NewConfig<TenantRowModel, TenantPropertySummaryResponseDto>()
            .Map(dest => dest.Id, src => src.PropertyPublicId)
            .Map(dest => dest.Name, src => src.PropertyName);

        config.NewConfig<TenantRowModel, TenantRoomSummaryResponseDto>()
            .Map(dest => dest.Id, src => src.RoomPublicId)
            .Map(dest => dest.Code, src => src.RoomCode)
            .Map(dest => dest.Name, src => src.RoomName);

        config.NewConfig<TenantJoinRoomRowModel, TenantPropertySummaryResponseDto>()
            .Map(dest => dest.Id, src => src.PropertyPublicId)
            .Map(dest => dest.Name, src => src.PropertyName);

        config.NewConfig<TenantJoinRoomRowModel, TenantRoomSummaryResponseDto>()
            .Map(dest => dest.Id, src => src.RoomPublicId)
            .Map(dest => dest.Code, src => src.RoomCode)
            .Map(dest => dest.Name, src => src.RoomName);

        config.NewConfig<TenantJoinRoomRowModel, TenantJoinRoomSummaryResponseDto>()
            .Map(dest => dest.Id, src => src.RoomPublicId)
            .Map(dest => dest.Name, src => src.RoomName);

        config.NewConfig<TenantJoinRoomRowModel, TenantJoinPreviewResponseDto>()
            .Map(dest => dest.Property, src => src.Adapt<TenantPropertySummaryResponseDto>())
            .Map(dest => dest.Room, src => src.Adapt<TenantJoinRoomSummaryResponseDto>());
    }
}
