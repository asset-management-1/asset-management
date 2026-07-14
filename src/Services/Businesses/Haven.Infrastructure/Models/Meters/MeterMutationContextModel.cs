namespace Haven.Infrastructure.Models.Meters;

/// <summary>
/// Carries the resolved state shared while electricity and water are applied in one transaction.
/// </summary>
/// <param name="Request">The validated meter mutation request.</param>
/// <param name="State">The locked room scope, tracked readings, policies, and previous confirmed values.</param>
/// <param name="Lookups">The typed master-data values required by the mutation.</param>
/// <param name="Periods">The selected and preceding calendar boundaries.</param>
/// <param name="BillingDate">The validated meter date converted to the persistence date type.</param>
/// <param name="CancellationToken">The token used to cancel the workflow.</param>
public sealed record MeterMutationContextModel(
    MeterMutationRequestModel Request,
    MeterMutationStateModel State,
    MeterMutationLookupModel Lookups,
    MeterPeriodsModel Periods,
    DateOnly BillingDate,
    CancellationToken CancellationToken);
