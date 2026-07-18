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
    /// Ensures each client session preserves its own valid Party selection.
    /// </summary>
    [Fact]
    public async Task ResolveSessionPartyIdAsync_Should_PreserveIndependentSessionContexts()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var partyTypeDefinition = CreateMasterDataType(1, "PartyType");
        var partyStatusDefinition = CreateMasterDataType(2, "PartyStatus");
        var tenantType = CreateMasterDataValue(10, partyTypeDefinition, "TENANT");
        var landlordType = CreateMasterDataValue(11, partyTypeDefinition, "LANDLORD");
        var activeStatus = CreateMasterDataValue(12, partyStatusDefinition, "ACTIVE");
        var tenantParty = CreateParty(20, tenantType, activeStatus);
        var landlordParty = CreateParty(21, landlordType, activeStatus);

        dbContext.MasterDataTypes.AddRange(partyTypeDefinition, partyStatusDefinition);
        dbContext.MasterDataValues.AddRange(tenantType, landlordType, activeStatus);
        dbContext.Parties.AddRange(tenantParty, landlordParty);
        dbContext.UserParties.AddRange(
            CreateUserParty(30, 100, tenantParty),
            CreateUserParty(31, 100, landlordParty));
        await dbContext.SaveChangesAsync();
        var sut = new UserPartyRepository(dbContext);

        // Act
        var tenantSession = await sut.ResolveSessionPartyIdAsync(100, tenantParty.Id);
        var landlordSession = await sut.ResolveSessionPartyIdAsync(100, landlordParty.Id);
        var unresolvedSession = await sut.ResolveSessionPartyIdAsync(100, null);

        // Assert
        tenantSession.Should().Be(tenantParty.Id);
        landlordSession.Should().Be(landlordParty.Id);
        unresolvedSession.Should().BeNull();
    }

    /// <summary>
    /// Ensures a Party value outside the canonical PartyType master-data type cannot become session context.
    /// </summary>
    [Fact]
    public async Task ResolveSessionPartyIdAsync_Should_IgnoreWrongPartyTypeDefinition()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var wrongTypeDefinition = CreateMasterDataType(1, "UnrelatedType");
        var partyStatusDefinition = CreateMasterDataType(2, "PartyStatus");
        var tenantType = CreateMasterDataValue(10, wrongTypeDefinition, "TENANT");
        var activeStatus = CreateMasterDataValue(11, partyStatusDefinition, "ACTIVE");
        var party = CreateParty(20, tenantType, activeStatus);

        dbContext.MasterDataTypes.AddRange(wrongTypeDefinition, partyStatusDefinition);
        dbContext.MasterDataValues.AddRange(tenantType, activeStatus);
        dbContext.Parties.Add(party);
        dbContext.UserParties.Add(CreateUserParty(30, 100, party));
        await dbContext.SaveChangesAsync();
        var sut = new UserPartyRepository(dbContext);

        // Act
        var result = await sut.ResolveSessionPartyIdAsync(100, party.Id);

        // Assert
        result.Should().BeNull();
    }

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
            Name = "LANDLORD",
            IsActive = true
        };
        var landlordType = new MasterDataValue
        {
            Id = 11,
            PublicId = Guid.NewGuid(),
            MasterDataTypeId = 1,
            Code = "LANDLORD",
            Name = "Chủ nhà",
            IsActive = true
        };
        var partyStatusType = new MasterDataType
        {
            Id = 2,
            PublicId = Guid.NewGuid(),
            Code = "PartyStatus",
            Name = "Party Status"
        };
        var activeStatus = new MasterDataValue
        {
            Id = 12,
            PublicId = Guid.NewGuid(),
            MasterDataTypeId = partyStatusType.Id,
            MasterDataType = partyStatusType,
            Code = "ACTIVE",
            Name = "Active",
            IsActive = true
        };
        var tenantParty = CreateParty(20, tenantType, activeStatus);
        var landlordParty = CreateParty(21, landlordType, activeStatus);

        dbContext.MasterDataTypes.Add(partyStatusType);
        dbContext.MasterDataValues.AddRange(tenantType, landlordType);
        dbContext.MasterDataValues.Add(activeStatus);
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
    /// Ensures inactive Parties cannot become a session context.
    /// </summary>
    [Fact]
    public async Task GetByUserIdAndPartyTypeAsync_Should_IgnoreInactiveParty()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var partyStatusType = new MasterDataType
        {
            Id = 1,
            PublicId = Guid.NewGuid(),
            Code = "PartyStatus",
            Name = "Party Status"
        };
        var inactiveStatus = new MasterDataValue
        {
            Id = 2,
            PublicId = Guid.NewGuid(),
            MasterDataTypeId = partyStatusType.Id,
            MasterDataType = partyStatusType,
            Code = "INACTIVE",
            Name = "Inactive",
            IsActive = true
        };
        var tenantType = new MasterDataValue
        {
            Id = 3,
            PublicId = Guid.NewGuid(),
            MasterDataTypeId = 4,
            Code = "TENANT",
            Name = "Tenant",
            IsActive = true
        };
        var party = CreateParty(5, tenantType, inactiveStatus);

        dbContext.MasterDataTypes.Add(partyStatusType);
        dbContext.MasterDataValues.AddRange(inactiveStatus, tenantType);
        dbContext.Parties.Add(party);
        dbContext.UserParties.Add(CreateUserParty(6, 7, party));
        await dbContext.SaveChangesAsync();
        var sut = new UserPartyRepository(dbContext);

        // Act
        var result = await sut.GetByUserIdAndPartyTypeAsync(7, "TENANT");

        // Assert
        result.Should().BeNull();
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
    /// Creates one canonical master-data type for repository predicates.
    /// </summary>
    /// <param name="id">The internal master-data type identifier.</param>
    /// <param name="code">The canonical type code.</param>
    /// <returns>The configured master-data type.</returns>
    private static MasterDataType CreateMasterDataType(long id, string code) =>
        new()
        {
            Id = id,
            PublicId = Guid.NewGuid(),
            Code = code,
            Name = code
        };

    /// <summary>
    /// Creates one active value under the supplied master-data type.
    /// </summary>
    /// <param name="id">The internal value identifier.</param>
    /// <param name="type">The owning master-data type.</param>
    /// <param name="code">The canonical value code.</param>
    /// <returns>The configured master-data value.</returns>
    private static MasterDataValue CreateMasterDataValue(
        long id,
        MasterDataType type,
        string code) =>
        new()
        {
            Id = id,
            PublicId = Guid.NewGuid(),
            MasterDataTypeId = type.Id,
            MasterDataType = type,
            Code = code,
            Name = code,
            IsActive = true
        };

    /// <summary>
    /// Creates a party linked to one canonical party-type value.
    /// </summary>
    /// <param name="id">The internal party identifier.</param>
    /// <param name="partyType">The canonical party-type value.</param>
    /// <param name="partyStatus">The canonical party-status value.</param>
    /// <returns>The configured party entity.</returns>
    private static Party CreateParty(
        long id,
        MasterDataValue partyType,
        MasterDataValue partyStatus) =>
        new()
        {
            Id = id,
            PublicId = Guid.NewGuid(),
            PartyTypeId = partyType.Id,
            PartyType = partyType,
            StatusId = partyStatus.Id,
            Status = partyStatus,
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
