namespace Be.Haven.Core.Extensions.Objects;

public static class QueryableExtension
{
    /// <summary>
    /// Represents the MethodInfo for <see cref="string.Contains(string)"/>.
    /// Used to build dynamic expressions that perform string containment checks,
    /// e.g. x.Name.Contains(searchTerm).
    /// </summary>
    private static readonly MethodInfo ContainsMethod =
        typeof(string).GetMethod("Contains", [typeof(string)]);

    /// <summary>
    /// Represents the MethodInfo for <see cref="string.ToLower()"/>.
    /// Commonly used in dynamic expressions to perform case-insensitive comparisons,
    /// by converting both the source and search term to lower case.
    /// </summary>
    private static readonly MethodInfo ToLowerMethod =
        typeof(string).GetMethod("ToLower", Type.EmptyTypes);

    /// <summary>
    /// Paginates the specified IQueryable by applying the given page size and page number.
    /// </summary>
    /// <param name="query">The queryable source to apply pagination on.</param>
    /// <param name="pageSize">The number of records to include per page. If less than or equal to zero, no pagination is applied.</param>
    /// <param name="pageNumber">The number of the page to retrieve. The first page is 1.</param>
    /// <typeparam name="TEntity">The type of the elements in the queryable source.</typeparam>
    /// <returns>
    /// An IQueryable that represents the paginated results of the original source.
    /// </returns>
    public static IQueryable<TEntity> Pagination<TEntity>(this IQueryable<TEntity> query, int pageSize, int pageNumber)
    {
        return pageSize <= 0 ? query : query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
    }

    /// <summary>
    /// Orders the elements of an IQueryable based on the specified order by string.
    /// </summary>
    /// <param name="query">The queryable source to apply ordering on.</param>
    /// <param name="orderBy">
    /// A comma-separated string specifying the fields to order by and their respective directions,
    /// e.g., "FieldName1 ASC, FieldName2 DESC".
    /// If the string is null or empty, no ordering is applied.
    /// </param>
    /// <typeparam name="TEntity">The type of the elements in the queryable source.</typeparam>
    /// <returns>
    /// An IQueryable that represents the ordered results of the original source.
    /// </returns>
    public static IQueryable<TEntity> OrderBy<TEntity>(this IQueryable<TEntity> query, string orderBy)
    {
        foreach (OrderByInfoResponse orderByInfo in ParseOrderBy(orderBy))
            query = ApplyOrderBy(query, orderByInfo);
        return query;
    }

    /// <summary>
    /// Applies an ordering to the specified IQueryable based on the provided OrderByInfoResponse.
    /// </summary>
    /// <param name="query">The queryable source to apply the ordering on.</param>
    /// <param name="orderByInfo">The information describing the property to order by, the sort direction, and whether it is the initial ordering.</param>
    /// <typeparam name="TEntity">The type of the elements in the queryable source.</typeparam>
    /// <returns>
    /// An IOrderedQueryable with the applied ordering.
    /// </returns>
    private static IQueryable<TEntity> ApplyOrderBy<TEntity>(IQueryable<TEntity> query,
        OrderByInfoResponse orderByInfo)
    {
        string[] props = orderByInfo.PropertyName.Split('.');
        Type type = typeof(TEntity);

        ParameterExpression arg = Expression.Parameter(type, "x");
        Expression expr = arg;
        foreach (string prop in props)
        {
            PropertyInfo pi = type.GetProperty(prop,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
            if (pi != null)
            {
                expr = Expression.Property(expr, pi);
                type = pi.PropertyType;
            }
        }

        Type delegateType = typeof(Func<,>).MakeGenericType(typeof(TEntity), type);
        LambdaExpression lambda = Expression.Lambda(delegateType, expr, arg);
        string methodName;

        if (!orderByInfo.Initial && query is IOrderedQueryable<TEntity>)
        {
            methodName = orderByInfo.Direction == SortDirection.Ascending ? QUERYABLE_THEN_BY : QUERYABLE_THEN_BY_DESCENDING;
        }
        else
        {
            methodName = orderByInfo.Direction == SortDirection.Ascending ? QUERYABLE_ORDER_BY : QUERYABLE_ORDER_BY_DESCENDING;
        }

        return (IOrderedQueryable<TEntity>)typeof(Queryable)
            .GetMethods()
            .Single(method => method.Name == methodName
                            && method.IsGenericMethodDefinition
                            && method.GetGenericArguments().Length == 2
                            && method.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(TEntity), type)
            .Invoke(null, [query, lambda]);
    }

    /// <summary>
    /// Parses the given OrderBy string into a collection of OrderByInfoResponse instances 
    /// that define the property name and direction for sorting.
    /// </summary>
    /// <param name="orderBy">The comma-separated OrderBy string containing property names and optional sort directions (ASC or DESC).</param>
    /// <returns>
    /// A collection of OrderByInfoResponse containing the parsed property names and sort directions.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the OrderBy string contains invalid formatting or property names.
    /// </exception>
    private static IEnumerable<OrderByInfoResponse> ParseOrderBy(string orderBy)
    {
        var validatedOrderByItems = ValidateOrderBy(orderBy);
        return ParseOrderByIterator(validatedOrderByItems);
    }

    /// <summary>
    /// Validates the given OrderBy string and ensures it is correctly formatted.
    /// </summary>
    /// <param name="orderBy">The OrderBy string to validate.</param>
    /// <returns>
    /// A string array of validated OrderBy items.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the OrderBy string contains invalid formatting or property names.
    /// </exception>
    private static string[] ValidateOrderBy(string orderBy)
    {
        if (string.IsNullOrEmpty(orderBy))
            return [];

        string[] items = orderBy.Split(',');

        foreach (string item in items)
        {
            string[] pair = item.Trim().Split(' ');

            if (pair.Length > 2)
            {
                throw new ArgumentException(string.Format(ERROR_INVALID_ORDER_BY_STRING, item));

            }

            string prop = pair[0].Trim();

            if (string.IsNullOrEmpty(prop))
            {
                throw new ArgumentException(ERROR_INVALID_PROPERTY);
            }
        }

        return items;
    }

    /// <summary>
    /// Iterates over the already validated OrderBy items and constructs a collection of OrderByInfoResponse.
    /// </summary>
    /// <param name="orderByItems">The validated OrderBy items.</param>
    /// <returns>
    /// An IEnumerable of OrderByInfoResponse containing the parsed property names and sort directions.
    /// </returns>
    private static IEnumerable<OrderByInfoResponse> ParseOrderByIterator(string[] orderByItems)
    {
        bool initial = true;

        foreach (string item in orderByItems)
        {
            string[] pair = item.Trim().Split(' ');
            string prop = pair[0].Trim();
            SortDirection dir = SortDirection.Ascending;

            if (pair.Length == 2 && "desc".Equals(pair[1].Trim(), StringComparison.OrdinalIgnoreCase))
            {
                dir = SortDirection.Descending;
            }

            yield return new OrderByInfoResponse
            {
                PropertyName = prop,
                Direction = dir,
                Initial = initial
            };

            initial = false;
        }
    }

    /// <summary>
    /// Applies dynamic filtering to an <see cref="IQueryable{T}"/> based on
    /// non-null properties of the provided request parameter.
    /// </summary>
    /// <typeparam name="TEntity">The entity type of the query.</typeparam>
    /// <typeparam name="TParams">The type of the filter parameter object.</typeparam>
    /// <param name="query">The original query to filter.</param>
    /// <param name="requestParameter">
    /// An object whose non-null properties will be matched against
    /// properties of <typeparamref name="TEntity"/> with the same name.
    /// </param>
    /// <returns>
    /// A filtered <see cref="IQueryable{T}"/> if any filters were applied;
    /// otherwise, the original <paramref name="query"/>.
    /// </returns>
    public static IQueryable<TEntity> Filter<TEntity, TParams>(
        this IQueryable<TEntity> query,
        TParams requestParameter)
    {
        Type paramsType = typeof(TParams);
        Type entityType = typeof(TEntity);

        PropertyInfo[] paramPropArr = paramsType.GetProperties();
        PropertyInfo[] entityPropArr = entityType.GetProperties();

        Expression mainExpression = null;
        var entityParameter = Expression.Parameter(entityType, entityType.Name.ToLower());

        foreach (PropertyInfo paramProp in paramPropArr)
        {
            object paramValue = paramProp.GetValue(requestParameter);
            PropertyInfo entityProp = entityPropArr.FirstOrDefault(n => n.Name.Equals(paramProp.Name));

            // Skip if the entity does not contain the property
            // or the parameter value is null/empty.
            if (entityProp is null || IsNullable(paramProp, paramValue))
            {
                continue;
            }

            ConstantExpression constantExpression = Expression.Constant(paramValue, entityProp.PropertyType);
            Expression entityMemberExpression = Expression.Property(entityParameter, entityProp);
            Expression subExpression = null;

            // For non-string properties, use exact equality.
            if (paramProp.PropertyType != typeof(string))
            {
                subExpression = Expression.Equal(entityMemberExpression, constantExpression);
            }
            else
            {
                // For string properties, use Contains (case-sensitive).
                subExpression = Expression.Call(entityMemberExpression, ContainsMethod, constantExpression);
            }

            if (subExpression is not null)
            {
                mainExpression = mainExpression is null
                    ? subExpression
                    : Expression.And(mainExpression, subExpression);
            }
        }

        return mainExpression is null
            ? query
            : query.Where(Expression.Lambda<Func<TEntity, bool>>(mainExpression, entityParameter));
    }

    /// <summary>
    /// Applies a text-based search over string properties of the entity type.
    /// Builds a dynamic OR expression using <c>Contains</c> on each target property.
    /// Also supports non-unicode comparison when configured.
    /// </summary>
    /// <typeparam name="TEntity">The entity type of the query.</typeparam>
    /// <param name="query">The original query to search.</param>
    /// <param name="searchTerm">
    /// The search keyword. If null or empty, the query is returned unchanged.
    /// </param>
    /// <param name="searchProps">
    /// Optional list of property names to restrict search to.
    /// If null or empty, all string properties of <typeparamref name="TEntity"/> are used.
    /// </param>
    /// <returns>
    /// A filtered <see cref="IQueryable{T}"/> containing entities that match
    /// the given <paramref name="searchTerm"/>; otherwise, the original query.
    /// </returns>
    public static IQueryable<TEntity> SearchTerm<TEntity>(
        this IQueryable<TEntity> query,
        string searchTerm,
        List<SearchFieldConfiguration> searchProps)
    {
        if (string.IsNullOrEmpty(searchTerm)) return query;

        searchTerm = searchTerm.ToLower();

        Type entityType = typeof(TEntity);
        PropertyInfo[] entityPropArr = entityType.GetProperties();

        // Limit to specific properties if searchProps is provided.
        if (searchProps != null && searchProps.Count > 0)
        {
            var fieldNames = searchProps.Select(s => s.FieldName).ToHashSet();

            entityPropArr = entityPropArr
                .Where(x => fieldNames.Contains(x.Name))
                .ToArray();
        }

        Expression mainExpression = null;
        var entityParameter = Expression.Parameter(entityType, entityType.Name.ToLower());
        var isEqualNonUnicode = searchTerm == searchTerm.ToNonUnicode();

        foreach (PropertyInfo entityProp in entityPropArr)
        {
            if (entityProp.PropertyType == typeof(string))
            {
                // Build expression: entityProp.ToLower().Contains(searchTerm)
                ConstantExpression constantExpression = Expression.Constant(searchTerm, entityProp.PropertyType);
                Expression entityMemberExpression = Expression.Property(entityParameter, entityProp);
                Expression lowerEntityMemberExpression = Expression.Call(entityMemberExpression, ToLowerMethod);
                Expression subExpression = Expression.Call(lowerEntityMemberExpression, ContainsMethod, constantExpression);

                mainExpression = mainExpression is null
                    ? subExpression
                    : Expression.Or(mainExpression, subExpression);

                // If unicode and non-unicode forms differ, search both variants.
                if (!isEqualNonUnicode)
                {
                    constantExpression = Expression.Constant(searchTerm.ToNonUnicode(), entityProp.PropertyType);
                    subExpression = Expression.Call(lowerEntityMemberExpression, ContainsMethod, constantExpression);
                    mainExpression = Expression.Or(mainExpression, subExpression);
                }
            }
        }

        return mainExpression == null
            ? query
            : query.Where(Expression.Lambda<Func<TEntity, bool>>(mainExpression, entityParameter));
    }

    /// <summary>
    /// Determines whether the given property value should be treated as "null"
    /// for filtering purposes.
    /// For non-string types, this checks for null.
    /// For string types, this checks for null or empty.
    /// </summary>
    /// <param name="prop">The property metadata.</param>
    /// <param name="value">The current value of the property.</param>
    /// <returns>
    /// <c>true</c> if the value is considered null/empty and should be ignored in filters;
    /// otherwise, <c>false</c>.
    /// </returns>
    private static bool IsNullable(PropertyInfo prop, object value)
    {
        if (prop.PropertyType != typeof(string))
        {
            return value is null;
        }

        return value is null || string.IsNullOrEmpty(value.ToString());
    }
}