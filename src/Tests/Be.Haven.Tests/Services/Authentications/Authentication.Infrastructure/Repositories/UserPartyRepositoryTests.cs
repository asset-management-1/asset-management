using Authentication.Domain.Entities;
using Authentication.Infrastructure.Context;
using Authentication.Infrastructure.Repositories;

namespace Be.Haven.Tests.Services.Authentications.Authentication.Infrastructure.Repositories;

/// <summary>
/// Verifies canonical party-type lookups in the authentication repository.
/// </summary>
public sealed class UserPartyRepositoryTests
{
    /// <summary>
    /// Ensures switch-party lookup matches the persisted code and never the display name.
    /// </summary>
    [Fact]
    public async Task GetByUserIdAndPartyTypeAsync_Should_MatchCode_NotDisplayName()
    {
        await using var dbContext = CreateDbContext();
        var tenantType = new MasterDataValue
        {
            Id = 10,
            PublicId = Guid.NewGuid(),
            MasterDataTypeId = 1,
            Code = "TENANT",
            Name = "LANDLORD"
        };
        var landlordType = new MasterDataValue
        {
            Id = 11,
            PublicId = Guid.NewGuid(),
            MasterDataTypeId = 1,
            Code = "LANDLORD",
            Name = "Chủ nhà"
        };
        var tenantParty = CreateParty(20, tenantType);
        var landlordParty = CreateParty(21, landlordType);

        dbContext.MasterDataValues.AddRange(tenantType, landlordType);
        dbContext.Parties.AddRange(tenantParty, landlordParty);
        dbContext.UserParties.AddRange(
            CreateUserParty(30, 100, tenantParty),
            CreateUserParty(31, 100, landlordParty));
        await dbContext.SaveChangesAsync();

        var repository = new UserPartyRepository(dbContext);
        var result = await repository.GetByUserIdAndPartyTypeAsync(100, "LANDLORD");

        result.Should().NotBeNull();
        result.PartyId.Should().Be(landlordParty.Id);
    }

    /// <summary>
    /// Creates an isolated in-memory authentication context for repository tests.
    /// </summary>
    /// <returns>The isolated authentication context.</returns>
    private static AuthenticationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AuthenticationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AuthenticationDbContext(options);
    }

    /// <summary>
    /// Creates a party linked to one canonical party-type value.
    /// </summary>
    /// <param name="id">The internal party identifier.</param>
    /// <param name="partyType">The canonical party-type value.</param>
    /// <returns>The configured party entity.</returns>
    private static Party CreateParty(long id, MasterDataValue partyType) =>
        new()
        {
            Id = id,
            PublicId = Guid.NewGuid(),
            PartyTypeId = partyType.Id,
            PartyType = partyType,
            StatusId = 1,
            DisplayName = $"Party {id}"
        };

    /// <summary>
    /// Creates a user-party link for the supplied account and party.
    /// </summary>
    /// <param name="id">The internal mapping identifier.</param>
    /// <param name="userId">The internal user identifier.</param>
    /// <param name="party">The linked party.</param>
    /// <returns>The configured user-party entity.</returns>
    private static UserParty CreateUserParty(long id, long userId, Party party) =>
        new()
        {
            Id = id,
            PublicId = Guid.NewGuid(),
            UserId = userId,
            PartyId = party.Id,
            Party = party
        };
}
