namespace Haven.Application.Extensions;

/// <summary>
/// Provides reusable master-data key and value helpers for Haven application workflows.
/// </summary>
public static class MasterDataExtensions
{
    /// <summary>
    /// Gets a resolved master-data value by type and code.
    /// </summary>
    /// <param name="values">The resolved master-data dictionary.</param>
    /// <param name="type">The master-data type taxonomy value.</param>
    /// <param name="code">The master-data value code.</param>
    /// <returns>The resolved master-data value.</returns>
    public static MasterDataValueModel GetValue(
        this IReadOnlyDictionary<MasterDataKeyModel, MasterDataValueModel> values,
        MasterDataTypeEnum type,
        string code)
    {
        ArgumentNullException.ThrowIfNull(values);

        var key = new MasterDataKeyModel(type, code);

        // Missing values mean the submitted input referenced an unsupported setup code.
        return values.TryGetValue(key, out var value)
            ? value
            : throw new ApiException(
                string.Format(CultureInfo.CurrentCulture, ApplicationErrorConstants.LookupErrors.ERROR_MASTER_DATA_NOT_FOUND, key.Type, code),
                BAD_REQUEST,
                StatusCodes.Status400BadRequest);
    }
}
