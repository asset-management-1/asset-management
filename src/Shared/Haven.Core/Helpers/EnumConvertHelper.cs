namespace Haven.Core.Helpers;

/// <summary>
/// The class allow to convert between primitive type to enum and vice versa.
/// </summary>
/// <typeparam name="T">The type of enum</typeparam>
public static class EnumConvertHelper<T> where T : Enum
{
    /// <summary>
    /// Allow convert from int to Enum
    /// </summary>
    /// <param name="value">The value of enum</param>
    /// <returns>The Enum</returns>
    /// <exception cref="ArgumentException">Indicate that enum is not contain the value</exception>
    public static T ConvertIntToEnum(int value)
    {
        if (Enum.IsDefined(typeof(T), value))
        {
            return (T)Enum.ToObject(typeof(T), value);
        }

        throw new ArgumentException(string.Format(NOT_DEFINE_IN_ENUM_ERROR, value));
    }

    /// <summary>
    /// Allow convert string to Enum
    /// </summary>
    /// <param name="enumName">The name of enum</param>
    /// <returns>The Enum</returns>
    /// <exception cref="ArgumentException">Indicate that enum is not contain the name</exception>
    public static T ConvertStringToEnum(string enumName)
    {
        if (Enum.IsDefined(typeof(T), enumName))
        {
            return (T)Enum.Parse(typeof(T), enumName);
        }

        throw new ArgumentException(string.Format(NOT_DEFINE_IN_ENUM_ERROR, enumName));
    }

    /// <summary>
    /// Allow get the enum name.
    /// </summary>
    /// <param name="enumType">Enum type want to get name.</param>
    /// <returns>The name of enum.</returns>
    public static string GetEnumName(T enumType)
    {
        return Enum.GetName(typeof(T), enumType);
    }

    /// <summary>
    /// Allow get the enum value.
    /// </summary>
    /// <param name="enumType">Enum type want to get value.</param>
    /// <returns>The value of enum.</returns>
    public static int GetEnumValue(T enumType)
    {
        var enumName = GetEnumName(enumType);
        return (int)Enum.Parse(typeof(T), enumName);
    }

    /// <summary>
    /// Get enum description from a string represent the enum name
    /// </summary>
    /// <param name="enumName">Enum name</param>
    /// <returns>Enum description</returns>
    public static string GetEnumDescription(string enumName)
    {
        var type = typeof(T);
        var memberInfo = type.GetMember(enumName);

        if (memberInfo.Length <= 0)
        {
            return string.Empty;
        }

        var attr = memberInfo[0].GetCustomAttribute<DescriptionAttribute>();
        return attr != null ? attr.Description : string.Empty;
    }
}