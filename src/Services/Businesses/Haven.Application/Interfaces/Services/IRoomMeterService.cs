namespace Haven.Application.Interfaces.Services;

/// <summary>
/// Defines landlord meter use cases for one room.
/// </summary>
public interface IRoomMeterService
{
    /// <summary>
    /// Loads one room meter month and its live invoice impact.
    /// </summary>
    /// <param name="request">The landlord-scoped room and selected calendar month.</param>
    /// <param name="cancellationToken">The token used to cancel the read.</param>
    /// <returns>The selected period, previous period, evidence, and current invoice impact.</returns>
    Task<MeterPeriodDetailResponseDto> GetPeriodAsync(
        MeterPeriodRequestModel request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads at most twelve room meter periods for one calendar year.
    /// </summary>
    /// <param name="request">The landlord-scoped room and selected calendar year.</param>
    /// <param name="cancellationToken">The token used to cancel the read.</param>
    /// <returns>The room meter history ordered from newest to oldest.</returns>
    Task<IReadOnlyList<MeterHistoryItemResponseDto>> GetHistoryAsync(
        MeterHistoryRequestModel request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves and confirms one complete room meter month using the requested mutation mode.
    /// </summary>
    /// <param name="request">The validated room meter values and optional evidence.</param>
    /// <param name="mode">The create or update existence rule enforced inside the protected transaction.</param>
    /// <param name="cancellationToken">The token used to cancel the mutation.</param>
    /// <returns>The committed room meter period.</returns>
    Task<MeterPeriodDetailResponseDto> SaveConfirmedPeriodAsync(
        MeterMutationRequestModel request,
        MeterMutationModeEnum mode,
        CancellationToken cancellationToken = default);
}
