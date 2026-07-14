namespace Haven.Application.Mappings.RoomPackages;

/// <summary>
/// Composes room package API responses from scoped room and effective package rows.
/// </summary>
public static class RoomPackageResponseMapper
{
    /// <summary>
    /// Builds the effective package list for one room.
    /// </summary>
    /// <param name="room">The scoped room header and common-package state.</param>
    /// <param name="packages">The effective user-managed package rows and ordered item rows.</param>
    /// <returns>The room package list response.</returns>
    public static RoomPackageListResponseDto MapList(
        RoomDetailRowModel room,
        IReadOnlyList<RoomPackageTemplateRowModel> packages)
    {
        // Reuse the room-detail package grouping so both APIs preserve identical item ordering and response fields.
        return new RoomPackageListResponseDto
        {
            Room = new RoomPackageRoomResponseDto
            {
                Id = room.UnitPublicId,
                Name = room.UnitName
            },
            UsesCommonPackages = room.UsesCommonPackages,
            Packages = RoomResponseMapper.MapPackageTemplates(packages)
        };
    }

    /// <summary>
    /// Builds one package detail in the selected room context.
    /// </summary>
    /// <param name="list">The already composed room package list context.</param>
    /// <param name="package">The selected package from that effective list.</param>
    /// <returns>The room package detail response.</returns>
    public static RoomPackageDetailResponseDto MapDetail(
        RoomPackageListResponseDto list,
        RoomPackageTemplateResponseDto package)
    {
        return new RoomPackageDetailResponseDto
        {
            Room = list.Room,
            UsesCommonPackages = list.UsesCommonPackages,
            Package = package
        };
    }
}
