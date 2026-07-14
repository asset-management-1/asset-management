namespace Haven.Application.Models.Properties.Mutation;

/// <summary>
/// Represents dependency counters for one package template.
/// </summary>
public class PackageTemplateMutationGuardModel
{
    /// <summary>
    /// Gets or sets the package public identifier being checked.
    /// </summary>
    public Guid PackagePublicId { get; set; }

    /// <summary>
    /// Gets or sets the number of active contracts referencing package rows with this code.
    /// </summary>
    public int ContractCount { get; set; }

    /// <summary>
    /// Gets a value indicating whether the package template is referenced by contracts.
    /// </summary>
    public bool HasDependencies => ContractCount > 0;
}
