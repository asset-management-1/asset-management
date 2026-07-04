namespace Haven.Application.Helpers;

/// <summary>
/// Generates unit codes for property structure setup.
/// </summary>
public static class PropertyUnitCodeHelper
{
    /// <summary>
    /// Generates a unit code from a room numbering pattern.
    /// </summary>
    /// <param name="pattern">The optional room numbering pattern.</param>
    /// <param name="floor">The floor number.</param>
    /// <param name="room">The room ordinal on the floor.</param>
    /// <returns>The generated unit code.</returns>
    public static string Generate(string pattern, int floor, int room)
    {
        var safePattern = pattern.NormalizeOptional() ?? DEFAULT_ROOM_NUMBERING_PATTERN;

        if (safePattern.Contains("{floor}", StringComparison.OrdinalIgnoreCase)
            || safePattern.Contains("{room}", StringComparison.OrdinalIgnoreCase))
        {
            return safePattern
                .Replace("{floor}", floor.ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase)
                .Replace("{room}", room.ToString("00", CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase);
        }

        return $"{floor}{room:00}";
    }
}
