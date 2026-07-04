namespace Haven.Infrastructure.Dependencies;

/// <summary>
/// Groups repositories used by property setup workflows.
/// </summary>
public class PropertyRepositoryDependencies
{
    /// <summary>
    /// Creates grouped property repositories.
    /// </summary>
    /// <param name="propertyRepository">The property repository.</param>
    /// <param name="unitRepository">The unit repository.</param>
    /// <param name="unitPackageRepository">The unit-package repository.</param>
    /// <param name="unitPackageItemRepository">The unit-package-item repository.</param>
    /// <param name="propertyPartyRepository">The property-party repository.</param>
    /// <param name="rentalChargePolicyRepository">The rental charge policy repository.</param>
    public PropertyRepositoryDependencies(
        IPropertyRepository propertyRepository,
        IUnitRepository unitRepository,
        IUnitPackageRepository unitPackageRepository,
        IUnitPackageItemRepository unitPackageItemRepository,
        IPropertyPartyRepository propertyPartyRepository,
        IRentalChargePolicyRepository rentalChargePolicyRepository)
    {
        PropertyRepository = propertyRepository;
        UnitRepository = unitRepository;
        UnitPackageRepository = unitPackageRepository;
        UnitPackageItemRepository = unitPackageItemRepository;
        PropertyPartyRepository = propertyPartyRepository;
        RentalChargePolicyRepository = rentalChargePolicyRepository;
    }

    /// <summary>
    /// Gets the property repository.
    /// </summary>
    public IPropertyRepository PropertyRepository { get; }

    /// <summary>
    /// Gets the unit repository.
    /// </summary>
    public IUnitRepository UnitRepository { get; }

    /// <summary>
    /// Gets the unit-package repository.
    /// </summary>
    public IUnitPackageRepository UnitPackageRepository { get; }

    /// <summary>
    /// Gets the unit-package-item repository.
    /// </summary>
    public IUnitPackageItemRepository UnitPackageItemRepository { get; }

    /// <summary>
    /// Gets the property-party repository.
    /// </summary>
    public IPropertyPartyRepository PropertyPartyRepository { get; }

    /// <summary>
    /// Gets the rental charge policy repository.
    /// </summary>
    public IRentalChargePolicyRepository RentalChargePolicyRepository { get; }
}
