namespace Authentication.Infrastructure.Repositories;

/// <summary>
/// Provides data access methods for document links.
/// </summary>
public class DocumentLinkRepository : GenericRepository<DocumentLink>, IDocumentLinkRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentLinkRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The authentication database context.</param>
    public DocumentLinkRepository(AuthenticationDbContext dbContext) : base(dbContext)
    {
    }
}
