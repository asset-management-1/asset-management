namespace Haven.Core.Extensions.Objects;

/// <summary>
/// Small set of common enum helpers: convert, parse, and read metadata.
/// </summary>
public static class EnumExtension
{
    /// <summary>
    /// Converts an enum value to its underlying <see cref="int"/> value.
    /// Works for any numeric underlying type (byte, short, int, etc.).
    /// </summary>
    /// <typeparam name="TEnum">Enum type.</typeparam>
    /// <param name="value">Enum value.</param>
    /// <returns>Integer representation of <paramref name="value"/>.</returns>
    public static int ToInt<TEnum>(this TEnum value) where TEnum : struct, Enum => Convert.ToInt32(value);

    /// <summary>
    /// Tries to convert an integer to a defined enum member.
    /// Returns <c>false</c> if <paramref name="raw"/> is not declared in the enum.
    /// </summary>
    /// <typeparam name="TEnum">Enum type.</typeparam>
    /// <param name="raw">Raw integer value.</param>
    /// <param name="value">Parsed enum value (when successful).</param>
    /// <returns><c>true</c> if <paramref name="raw"/> maps to a defined member; otherwise <c>false</c>.</returns>
    public static bool TryFromInt<TEnum>(int raw, out TEnum value) where TEnum : struct, Enum
    {
        var tmp = (TEnum)Enum.ToObject(typeof(TEnum), raw);
        if (Enum.IsDefined(typeof(TEnum), tmp))
        {
            value = tmp;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Tries to parse a string to a defined enum member (case-insensitive).
    /// Returns <c>false</c> if the parsed value is not declared in the enum.
    /// </summary>
    /// <typeparam name="TEnum">Enum type.</typeparam>
    /// <param name="name">Enum member name (any casing).</param>
    /// <param name="value">Parsed enum value (when successful).</param>
    /// <returns><c>true</c> if <paramref name="name"/> matches a declared member; otherwise <c>false</c>.</returns>
    public static bool TryParseIgnoreCase<TEnum>(string name, out TEnum value) where TEnum : struct, Enum =>
        Enum.TryParse(name, true, out value) && Enum.IsDefined(typeof(TEnum), value);

    /// <summary>
    /// Gets the <see cref="DescriptionAttribute"/> text of an enum value.
    /// Falls back to the enum member name if no description is present.
    /// </summary>
    /// <typeparam name="TEnum">Enum type.</typeparam>
    /// <param name="value">Enum value.</param>
    /// <returns>Description text or the member name.</returns>
    public static string GetDescription<TEnum>(this TEnum value) where TEnum : struct, Enum
    {
        var type = typeof(TEnum);
        var name = Enum.GetName(type, value);
        if (string.IsNullOrEmpty(name)) return value.ToString();

        var mi = type.GetMember(name).FirstOrDefault();
        return mi?.GetCustomAttribute<DescriptionAttribute>()?.Description ?? name;
    }

    /// <summary>
    /// Gets the <see cref="DisplayAttribute.Name"/> of an enum value.
    /// Falls back to the enum member name if no display name is present.
    /// </summary>
    /// <typeparam name="TEnum">Enum type.</typeparam>
    /// <param name="value">Enum value.</param>
    /// <returns>Display name or the member name.</returns>
    public static string GetDisplayName<TEnum>(this TEnum value) where TEnum : struct, Enum
    {
        var type = typeof(TEnum);
        var name = Enum.GetName(type, value);
        if (string.IsNullOrEmpty(name)) return value.ToString();

        var mi = type.GetMember(name).FirstOrDefault();
        return mi?.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? name;
    }

    /// <summary>
    /// Builds a dictionary for UI/dropdowns: key = int value, value = description (or name).
    /// </summary>
    /// <typeparam name="TEnum">Enum type.</typeparam>
    /// <param name="useDescription">
    /// When <c>true</c>, uses <see cref="DescriptionAttribute"/>; otherwise uses the member name.
    /// </param>
    /// <returns>Dictionary mapping numeric value to display text.</returns>
    public static Dictionary<int, string> ToDictionary<TEnum>(bool useDescription = true) where TEnum : struct, Enum =>
        Enum.GetValues(typeof(TEnum))
            .Cast<TEnum>()
            .ToDictionary(e => e.ToInt(), e => useDescription ? e.GetDescription() : e.ToString());
}