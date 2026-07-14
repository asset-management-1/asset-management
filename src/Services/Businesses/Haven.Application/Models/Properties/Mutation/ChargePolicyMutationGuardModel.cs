namespace Haven.Application.Models.Properties.Mutation;

/// <summary>
/// Represents dependency counters for one rental charge policy.
/// </summary>
public class ChargePolicyMutationGuardModel
{
    /// <summary>
    /// Gets or sets the charge policy public identifier.
    /// </summary>
    public Guid PolicyPublicId { get; set; }

    /// <summary>
    /// Gets or sets the number of invoice lines referencing the policy.
    /// </summary>
    public int InvoiceLineCount { get; set; }

    /// <summary>
    /// Gets a value indicating whether the policy is referenced by billing data.
    /// </summary>
    public bool HasDependencies => InvoiceLineCount > 0;
}
