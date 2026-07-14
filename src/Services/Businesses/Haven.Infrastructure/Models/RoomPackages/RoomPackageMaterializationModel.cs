namespace Haven.Infrastructure.Models.RoomPackages;

/// <summary>
/// Carries the room-owned package copies and package codes created during one materialization phase.
/// </summary>
public sealed record RoomPackageMaterializationModel(
    IReadOnlyDictionary<Guid, UnitPackage> SourceToRoomPackage,
    ISet<string> UsedCodes);
