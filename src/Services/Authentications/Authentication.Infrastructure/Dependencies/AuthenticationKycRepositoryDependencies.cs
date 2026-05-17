namespace Authentication.Infrastructure.Dependencies;

/// <summary>
/// Groups repositories used by KYC and private document persistence.
/// </summary>
public class AuthenticationKycRepositoryDependencies
{
    /// <summary>
    /// Groups repositories that persist KYC documents, identifiers, and document links.
    /// </summary>
    /// <param name="documentRepository">The document repository.</param>
    /// <param name="documentLinkRepository">The document-link repository.</param>
    /// <param name="partyIdentifierRepository">The party-identifier repository.</param>
    public AuthenticationKycRepositoryDependencies(
        IDocumentRepository documentRepository,
        IDocumentLinkRepository documentLinkRepository,
        IPartyIdentifierRepository partyIdentifierRepository)
    {
        DocumentRepository = documentRepository;
        DocumentLinkRepository = documentLinkRepository;
        PartyIdentifierRepository = partyIdentifierRepository;
    }

    /// <summary>
    /// Gets the document repository.
    /// </summary>
    public IDocumentRepository DocumentRepository { get; }

    /// <summary>
    /// Gets the document-link repository.
    /// </summary>
    public IDocumentLinkRepository DocumentLinkRepository { get; }

    /// <summary>
    /// Gets the party-identifier repository.
    /// </summary>
    public IPartyIdentifierRepository PartyIdentifierRepository { get; }

}
