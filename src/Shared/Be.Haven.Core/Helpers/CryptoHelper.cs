namespace Be.Haven.Core.Helpers;

public static class CryptoHelper
{
    /// <summary>
    /// Generates an HMAC-SHA256 of <paramref name="rawData"/> using a HEX secret key,
    /// returns uppercase hexadecimal string. Data is encoded as ISO-8859-1 (Latin-1).
    /// </summary>
    public static string GetSecureHash(string rawData, string secureSecret)
    {
        // Parse HEX -> key bytes
        var key = Convert.FromHexString(secureSecret);

        try
        {
            // Encode data to Latin-1 to match legacy behavior
            var data = Latin1.GetBytes(rawData);

            using var hmac = new HMACSHA256(key);
            var hash = hmac.ComputeHash(data);

            // Uppercase hex without extra allocations
            return Convert.ToHexString(hash);
        }
        finally
        {
            // Best-effort: zero key material
            CryptographicOperations.ZeroMemory(key);
        }
    }
}