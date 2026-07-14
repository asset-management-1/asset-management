namespace Haven.Infrastructure.Mappings.RentalChargePolicies;

/// <summary>
/// Applies shared rental charge policy fields from property and room charge-policy inputs.
/// </summary>
public static class RentalChargePolicyFieldMapper
{
    /// <summary>
    /// Applies common charge-policy fields to a tracked or new rental charge policy entity.
    /// </summary>
    /// <param name="policy">The rental charge policy entity.</param>
    /// <param name="input">The charge-policy input.</param>
    /// <param name="lookups">The resolved lookup identifiers needed by the policy input.</param>
    public static void Apply<TInput>(
        RentalChargePolicy policy,
        TInput input,
        RentalChargePolicyLookupModel lookups)
        where TInput : IChargePolicyInput
    {
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(lookups);

        // Mapster merges submitted scalar values and preserves omitted nullable fields on tracked policies.
        input.Adapt(policy);

        // Master-data identifiers and calculation behavior stay explicit because they are not flat DTO fields.
        if (lookups.LineTypeId.HasValue)
        {
            policy.LineTypeId = lookups.LineTypeId.Value;
        }

        if (input.Code is not null)
        {
            policy.VehicleTypeId = string.Equals(
                input.Code,
                MASTER_CODE_INVOICE_LINE_TYPE_PARKING,
                StringComparison.OrdinalIgnoreCase)
                ? lookups.VehicleTypeId
                : null;
        }
        else if (input.VehicleTypeCode is not null)
        {
            policy.VehicleTypeId = lookups.VehicleTypeId;
        }

        if (input.CalculationMethodCode is not null)
        {
            policy.IsUsageBased = string.Equals(
                input.CalculationMethodCode,
                CALCULATION_METHOD_METER_READING,
                StringComparison.OrdinalIgnoreCase);
        }

        if (lookups.StatusId.HasValue)
        {
            policy.StatusId = lookups.StatusId.Value;
        }
    }
}
