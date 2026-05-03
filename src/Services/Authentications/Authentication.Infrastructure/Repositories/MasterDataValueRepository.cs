namespace Authentication.Infrastructure.Repositories;

/// <summary>
/// Provides data access methods for master data values.
/// </summary>
public class MasterDataValueRepository : GenericRepository<MasterDataValue>, IMasterDataValueRepository
{
    private readonly AuthenticationDbContext _authenticationDbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="MasterDataValueRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The authentication database context.</param>
    public MasterDataValueRepository(AuthenticationDbContext dbContext) : base(dbContext)
    {
        _authenticationDbContext = dbContext;
    }

    /// <summary>
    /// Gets an active master data value by master data type and value.
    /// </summary>
    /// <param name="type">The master data type code or name.</param>
    /// <param name="value">The master data value code or name.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The matched active master data value; otherwise <c>null</c>.</returns>
    public Task<MasterDataValue> GetByTypeAndValueAsync(
        string type,
        string value,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(value))
        {
            return Task.FromResult<MasterDataValue>(null);
        }

        var normalizedType = type.Trim();
        var normalizedValue = value.Trim();

        // Match by type code/name and value code/name to support flexible master data lookup.
        return _authenticationDbContext.MasterDataValues
            .AsNoTracking()
            .Include(x => x.MasterDataType)
            .FirstOrDefaultAsync(
                x => !x.IsDeleted
                     && x.IsActive
                     && !x.MasterDataType.IsDeleted
                     && (x.Code == normalizedValue || x.Name == normalizedValue)
                     && (x.MasterDataType.Code == normalizedType || x.MasterDataType.Name == normalizedType),
                cancellationToken);
    }
}