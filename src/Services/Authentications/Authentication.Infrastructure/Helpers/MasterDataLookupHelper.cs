namespace Authentication.Infrastructure.Helpers;

/// <summary>
/// Provides shared master-data lookup helpers for authentication infrastructure flows.
/// </summary>
internal static class MasterDataLookupHelper
{
    /// <summary>
    /// Reads a required master-data value from a resolved batch lookup.
    /// </summary>
    /// <param name="masterDataValues">The resolved master-data values keyed by lookup type/value.</param>
    /// <param name="request">The required master-data lookup and failure metadata.</param>
    /// <returns>The resolved master-data value.</returns>
    public static MasterDataValue GetRequired(
        IReadOnlyDictionary<MasterDataValueLookupModel, MasterDataValue> masterDataValues,
        MasterDataRequiredLookupModel request)
    {
        // Missing master data maps to either a business error or a server configuration error.
        var lookup = new MasterDataValueLookupModel(request.Type, request.Value);
        if (masterDataValues.TryGetValue(lookup, out var masterDataValue))
        {
            return masterDataValue;
        }

        if (!string.IsNullOrWhiteSpace(request.ErrorCode))
        {
            throw new ApiException(request.Message, request.ErrorCode, request.StatusCode);
        }

        throw new HttpStatusCodeException(request.Message, request.StatusCode);
    }
}
