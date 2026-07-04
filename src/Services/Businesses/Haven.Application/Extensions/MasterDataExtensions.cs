namespace Haven.Application.Extensions;

/// <summary>
/// Provides reusable master-data key and value helpers for Haven application workflows.
/// </summary>
public static class MasterDataExtensions
{
    /// <summary>
    /// Adds a required master-data key.
    /// </summary>
    /// <param name="keys">The target key set.</param>
    /// <param name="type">The master-data type code.</param>
    /// <param name="code">The master-data value code.</param>
    public static void AddKey(this ISet<MasterDataKeyModel> keys, string type, string code)
    {
        ArgumentNullException.ThrowIfNull(keys);

        keys.Add(new MasterDataKeyModel(type, code));
    }

    /// <summary>
    /// Adds an optional master-data key when a code is supplied.
    /// </summary>
    /// <param name="keys">The target key set.</param>
    /// <param name="type">The master-data type code.</param>
    /// <param name="code">The optional master-data value code.</param>
    public static void AddOptionalKey(this ISet<MasterDataKeyModel> keys, string type, string code)
    {
        ArgumentNullException.ThrowIfNull(keys);

        var normalizedCode = code.NormalizeOptional();
        if (normalizedCode is not null)
        {
            keys.Add(new MasterDataKeyModel(type, normalizedCode));
        }
    }

    /// <summary>
    /// Gets a required master-data value from a resolved value dictionary.
    /// </summary>
    /// <param name="values">The resolved master-data dictionary.</param>
    /// <param name="key">The required master-data key.</param>
    /// <returns>The resolved master-data value.</returns>
    public static MasterDataValueModel RequireValue(
        this IReadOnlyDictionary<MasterDataKeyModel, MasterDataValueModel> values,
        MasterDataKeyModel key)
    {
        ArgumentNullException.ThrowIfNull(values);
        ArgumentNullException.ThrowIfNull(key);

        // Missing master-data is a request/setup problem, so translate it into the standard API error.
        return values.TryGetValue(key, out var value)
            ? value
            : throw new ApiException(
                string.Format(CultureInfo.CurrentCulture, ERROR_MASTER_DATA_NOT_FOUND, key.Type, key.Code),
                BAD_REQUEST,
                StatusCodes.Status400BadRequest);
    }

    /// <summary>
    /// Gets a required master-data value by type and code.
    /// </summary>
    /// <param name="values">The resolved master-data dictionary.</param>
    /// <param name="type">The master-data type code.</param>
    /// <param name="code">The master-data value code.</param>
    /// <returns>The resolved master-data value.</returns>
    public static MasterDataValueModel RequireValue(
        this IReadOnlyDictionary<MasterDataKeyModel, MasterDataValueModel> values,
        string type,
        string code)
    {
        return values.RequireValue(new MasterDataKeyModel(type, code));
    }
}
