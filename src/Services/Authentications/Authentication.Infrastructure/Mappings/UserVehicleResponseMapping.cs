namespace Authentication.Infrastructure.Mappings;

/// <summary>
/// Registers Mapster mappings for tenant vehicle response projections.
/// </summary>
public class UserVehicleResponseMapping : IRegister
{
    /// <summary>
    /// Registers vehicle read-model mappings into public API response DTOs.
    /// </summary>
    /// <param name="config">The Mapster configuration instance.</param>
    public void Register(TypeAdapterConfig config)
    {
        // Vehicle read models carry raw master-data codes; Mapster normalizes only the public enum fields.
        config.NewConfig<UserVehicleReadModel, UserVehicleResponseDto>()
            .Map(dest => dest.VehicleType, src => ApiEnumContractMapper.ToRequiredVehicleType(src.VehicleType))
            .Map(dest => dest.Status, src => ApiEnumContractMapper.ToRequiredVehicleStatus(src.Status));
    }
}
