namespace Haven.Infrastructure.Mappings.MasterData;

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
        // Repository aliases SQL rows to this shape, then Mapster projects only the application value fields.
        config.NewConfig<MasterDataRowModel, MasterDataValueModel>();
    }
}
