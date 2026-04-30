namespace Be.Haven.ApiCommon.Filters;

/// <summary>
/// This filter modifies the Swagger documentation by standardizing the naming
/// of path parameters to use kebab-case where possible. It ensures consistency
/// in route parameter naming conventions by eliminating duplicates between
/// camelCase and kebab-case variants of the same parameter.
/// </summary>
public sealed class KebabCaseSwaggerFilter : IOperationFilter
{
    /// <summary>
    /// Modifies the OpenAPI operation by filtering parameters to ensure consistency with kebab-case naming conventions.
    /// Removes camelCase parameters if their kebab-case counterparts exist in the same operation.
    /// </summary>
    /// <param name="operation">The OpenAPI operation to be modified.</param>
    /// <param name="context">The context for the operation filter, providing additional metadata.</param>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // No parameters to process
        if (operation.Parameters == null || operation.Parameters.Count == 0) return;

        // Only care about path (route) parameters: /resource/{id}
        var pathParams = operation.Parameters
                                  .Where(p => p.In == ParameterLocation.Path)
                                  .ToList();

        // Nothing to dedupe if there is 0 or 1 path param
        if (pathParams.Count <= 1) return;

        // Build a case-insensitive set of existing path parameter names
        var names = new HashSet<string>(
            pathParams.Select(p => p.Name),
            StringComparer.OrdinalIgnoreCase);

        // Keep non-path params as-is; for path params, drop camelCase if kebab-case exists
        operation.Parameters = operation.Parameters
                                        .Where(p =>
                                        {
                                            // Leave query/header/cookie/body params untouched
                                            if (p.In != ParameterLocation.Path) return true;

                                            // Already kebab-case => keep
                                            if (p.Name.Contains('-')) return true;

                                            // Convert camelCase -> kebab-case
                                            var kebab = StringConvertHelper.ToKebabCase(p.Name);

                                            // If the kebab-case param already exists, remove the camelCase one
                                            return !names.Contains(kebab);
                                        })
                                        .ToList();
    }
}