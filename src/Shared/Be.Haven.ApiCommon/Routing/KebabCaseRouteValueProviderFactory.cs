namespace Be.Haven.ApiCommon.Routing;

/// <summary>
/// Represents a factory that provides a value provider which processes route values with kebab-case keys
/// and transforms them to camelCase keys, unless they exist in a reserved list.
/// This factory is intended to integrate with ASP.NET Core's model binding system.
/// </summary>
public sealed class KebabCaseRouteValueProviderFactory : IValueProviderFactory
{
    private static readonly HashSet<string> Reserved =
        new(StringComparer.OrdinalIgnoreCase) { "version", "controller", "action", "area" };

    /// <summary>
    /// Creates a value provider based on the provided context by processing route values
    /// and transforming kebab-case keys to camelCase if they are not in the reserved list.
    /// </summary>
    /// <param name="context">The context used to create the value provider, containing route data and related information.</param>
    /// <returns>
    /// A task that represents the asynchronous operation of creating a value provider.
    /// The task is completed when all relevant route values are processed.
    /// </returns>
    public Task CreateValueProviderAsync(ValueProviderFactoryContext context)
    {
        var values = context.ActionContext.RouteData.Values;
        if (values.Count == 0) return Task.CompletedTask;

        // avoid modifying while iterating
        var keys = values.Keys.ToList();

        foreach (var key in keys)
        {
            if (string.IsNullOrWhiteSpace(key)) continue;
            if (Reserved.Contains(key)) continue;

            // only kebab-case keys
            if (!key.Contains('-')) continue;

            var camel = StringConvertHelper.ToCamelCase(key);
            if (!values.ContainsKey(camel))
            {
                values[camel] = values[key];
            }
        }

        return Task.CompletedTask;
    }
}