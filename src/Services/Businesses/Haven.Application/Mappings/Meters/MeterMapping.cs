namespace Haven.Application.Mappings.Meters;

/// <summary>
/// Registers flat meter command and read-row mappings.
/// </summary>
public sealed class MeterMapping : IRegister
{
    /// <summary>
    /// Registers mappings while leaving calculations and lifecycle decisions to the service.
    /// </summary>
    /// <param name="config">The Mapster configuration instance.</param>
    // ReSharper disable once MemberCanBeMadeStatic.Global
    // Mapster discovers IRegister implementations through assembly scanning and invokes this interface method.
    public void Register(TypeAdapterConfig config)
    {
        // Commands and service requests share the same validated flat meter fields.
        config.NewConfig<CreateMeterCommand, MeterMutationRequestModel>();
        config.NewConfig<UpdateMeterCommand, MeterMutationRequestModel>();

        // Read rows map persisted scalar values; business calculations stay in the service and multi-source composition stays in MeterResponseMapper.
        config.NewConfig<MeterRowModel, MeterValueResponseDto>()
            .Map(destination => destination.Id, source => source.MeterPublicId)
            .Map(destination => destination.Previous, source => source.PreviousReading)
            .Map(destination => destination.Current, source => source.CurrentReading)
            .Map(destination => destination.SuggestedPrice, source => source.SuggestedUnitPrice)
            .Map(destination => destination.Price, source => source.UnitPriceSnapshot)
            .Map(destination => destination.Date, source => source.ReadingDate)
            .Map(
                destination => destination.PreviousSourceCode,
                source => source.HasPriorConfirmedReading
                    ? UTILITY_PREVIOUS_SOURCE_PRIOR_CONFIRMED
                    : UTILITY_PREVIOUS_SOURCE_MANUAL)
            .Ignore(destination => destination.Amount)
            .Ignore(destination => destination.Evidence);
    }
}
