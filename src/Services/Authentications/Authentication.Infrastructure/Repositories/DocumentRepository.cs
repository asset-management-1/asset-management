namespace Authentication.Infrastructure.Repositories;

/// <summary>
/// Provides data access methods for document metadata.
/// </summary>
public class DocumentRepository : GenericRepository<Document>, IDocumentRepository
{
    /// <summary>
    /// Initialises a new instance of the <see cref="DocumentRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The authentication database context.</param>
    public DocumentRepository(AuthenticationDbContext dbContext) : base(dbContext)
    {
    }
}
