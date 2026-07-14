namespace Authentication.Infrastructure.Helpers;

/// <summary>
/// Provides shared master-data value helpers for authentication infrastructure flows.
/// </summary>
internal static class MasterDataValueHelper
{
    /// <summary>
    /// Reads a required master-data value from a resolved batch key set.
    /// </summary>
    /// <param name="masterDataValues">The resolved master-data values keyed by requested type/value.</param>
    /// <param name="request">The required master-data key and failure metadata.</param>
    /// <returns>The resolved master-data value.</returns>
    public static MasterDataValue GetRequired(
        IReadOnlyDictionary<MasterDataValueKeyModel, MasterDataValue> masterDataValues,
        MasterDataValueRequirementModel request)
    {
        // Missing master data maps to either a business error or a server configuration error.
        var key = new MasterDataValueKeyModel(request.Type, request.Value);

        if (masterDataValues.TryGetValue(key, out var masterDataValue))
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
