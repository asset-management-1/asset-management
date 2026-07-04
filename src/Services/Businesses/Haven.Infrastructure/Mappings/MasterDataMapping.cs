namespace Haven.Infrastructure.Mappings;

/// <summary>
/// Registers infrastructure mappings for master-data read rows.
/// </summary>
public class MasterDataMapping : IRegister
{
    /// <summary>
    /// Registers master-data row projections.
    /// </summary>
    /// <param name="config">The Mapster configuration instance.</param>
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<MasterDataRowModel, MasterDataValueModel>();
    }
}
