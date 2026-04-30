using JsonProperty = Newtonsoft.Json.Serialization.JsonProperty;

namespace Be.Haven.Core.Extensions.Serializations;

/// <summary>
/// Custom contract resolver that ignores the JsonProperty attribute and uses the member name as the property name.
/// </summary>
public class IgnorePropertyContractResolver : CamelCasePropertyNamesContractResolver
{
    /// <summary>
    /// Creates a JsonProperty for the given member, ignoring the JsonProperty attribute and using the member name as the property name.
    /// </summary>
    /// <param name="member">The member to create a property for.</param>
    /// <param name="memberSerialization">The member serialization options.</param>
    /// <returns>A JsonProperty for the given member.</returns>
    protected override JsonProperty CreateProperty(
        MemberInfo member,
        MemberSerialization memberSerialization)
    {
        var prop = base.CreateProperty(member, memberSerialization);

        // Ignore attribute JsonProperty and priority default
        prop.PropertyName = member.Name;
        return prop;
    }
}