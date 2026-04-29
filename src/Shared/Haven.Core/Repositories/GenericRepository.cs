namespace Haven.Core.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly DbContext _dbContext;

    /// <summary>
    /// Represents a generic repository that provides basic data access operations for entities of a specified type.
    /// </summary>
    /// <typeparam name="T">The type of entity the repository will operate on, which must be a class.</typeparam>
    public GenericRepository(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Asynchronously adds a new entity to the database.
    /// </summary>
    /// <param name="entity">The entity to be added to the database.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the entity that was added.</returns>
    public virtual async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        await _dbContext.Set<T>().AddAsync(entity, ct);
        return entity;
    }

    /// <summary>
    /// Adds a range of entities to the database context asynchronously (not yet saved).
    /// </summary>
    /// <param name="entities">The list of entities to be added to the database context.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    public virtual async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
    {
        await _dbContext.Set<T>().AddRangeAsync(entities, ct);
    }

    /// <summary>
    /// Asynchronously updates an existing entity in the database context with the new state provided.
    /// </summary>
    /// <param name="entity">The entity with updated values to be committed to the database.</param>
    /// <returns>A task representing the asynchronous operation, which does not produce any value.</returns>
    public virtual Task UpdateAsync(T entity)
    {
        var entry = _dbContext.Entry(entity);
        if (entry.State == EntityState.Detached)
        {
            _dbContext.Set<T>().Attach(entity);
            entry.State = EntityState.Modified;
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Updates a range of entities in the database by marking them as modified (not yet saved).
    /// </summary>
    /// <param name="entities">A list of entities to be updated in the database.</param>
    /// <returns>A task representing the asynchronous operation of updating the entities.</returns>
    public virtual Task UpdateRangeAsync(IEnumerable<T> entities)
    {
        foreach (var entity in entities)
        {
            var entry = _dbContext.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                _dbContext.Set<T>().Attach(entity);
                entry.State = EntityState.Modified;
            }
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously deletes the specified entity from the repository.
    /// </summary>
    /// <param name="entity">The entity to be deleted from the repository.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual Task DeleteAsync(T entity)
    {
        _dbContext.Set<T>().Remove(entity);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Deletes a collection of entities from the database context asynchronously.
    /// </summary>
    /// <param name="entities">The list of entities to be deleted.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    public virtual Task DeleteRangeAsync(List<T> entities)
    {
        _dbContext.Set<T>().RemoveRange(entities);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously retrieves an entity of the specified type by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to retrieve.</param>
    /// <returns>A task representing the asynchronous operation, containing the entity if found; otherwise, null.</returns>
    public virtual async Task<T> GetByIdAsync(int id)
    {
        return await _dbContext.Set<T>().FindAsync(id);
    }

    /// <summary>
    /// Asynchronously retrieves the first entity that matches the specified predicate (read-only).
    /// </summary>
    /// <param name="predicate">The filter expression used to match an entity.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the first matching entity or null.</returns>
    public virtual async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        return await _dbContext.Set<T>()
                               .AsNoTracking()
                               .FirstOrDefaultAsync(predicate, ct);
    }

    /// <summary>
    /// Determines asynchronously whether any entity matches the specified predicate.
    /// </summary>
    /// <param name="predicate">The filter expression to evaluate against entities.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing true if any entity matches; otherwise, false.</returns>
    public virtual Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        return _dbContext.Set<T>().AnyAsync(predicate, ct);
    }

    /// <summary>
    /// Counts asynchronously the number of entities that match the specified predicate (optional).
    /// </summary>
    /// <param name="predicate">An optional filter expression. If null, counts all entities.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the number of matching entities.</returns>
    public virtual Task<int> CountAsync(Expression<Func<T, bool>> predicate = null, CancellationToken ct = default)
    {
        return predicate is null
            ? _dbContext.Set<T>().CountAsync(ct)
            : _dbContext.Set<T>().CountAsync(predicate, ct);
    }

    /// <summary>
    /// Retrieves all entities of type <typeparamref name="T"/> from the database (read-only).
    /// </summary>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing a read-only list of all entities.</returns>
    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default)
    {
        return await _dbContext
                     .Set<T>()
                     .AsNoTracking()
                     .ToListAsync(ct);
    }

    /// <summary>
    /// Retrieves a list of entities that satisfy the given predicate (read-only).
    /// </summary>
    /// <param name="predicate">The filter expression to evaluate against entities.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing a list of matching entities.</returns>
    public virtual async Task<IReadOnlyList<T>> GetListAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        return await _dbContext.Set<T>()
                               .AsNoTracking()
                               .Where(predicate)
                               .ToListAsync(ct);
    }
    
    /// <summary>
    /// Asynchronously retrieves a list of projected results of type <typeparamref name="TResult"/> that satisfy the specified criteria.
    /// </summary>
    /// <param name="predicate">An expression that specifies the filtering criteria to apply to the entities of type <typeparamref name="T"/>.</param>
    /// <param name="selector">An expression that projects the entity of type <typeparamref name="T"/> into a result of type <typeparamref name="TResult"/>.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <typeparam name="TResult">The type of the result projected from the entity of type <typeparamref name="T"/>.</typeparam>
    /// <returns>A task that represents the asynchronous operation. The task result contains a read-only list of projected results of type <typeparamref name="TResult"/>.</returns>
    public virtual async Task<IReadOnlyList<TResult>> GetListAsync<TResult>(
        Expression<Func<T, bool>> predicate,
        Expression<Func<T, TResult>> selector,
        CancellationToken ct = default)
    {
        return await _dbContext.Set<T>()
                               .AsNoTracking()
                               .Where(predicate)
                               .Select(selector)
                               .ToListAsync(ct);
    }
    
    /// <summary>
    /// Retrieves a paginated response of entities based on the specified parameters.
    /// </summary>
    /// <typeparam name="TParam">The type of parameter used to provide pagination and sorting information, extending <see cref="BaseParameterRequest"/>.</typeparam>
    /// <param name="parameter">The request parameter that contains pagination and sorting details.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing a <see cref="PaginationResponse{T}"/> with a list of entities and total count.</returns>
    public virtual async Task<PaginationResponse<IReadOnlyList<T>>> GetPagedReponseAsync<TParam>(TParam parameter, CancellationToken ct = default)
        where TParam : BaseParameterRequest
    {
        var response = new PaginationResponse<IReadOnlyList<T>>(parameter.PageNumber, parameter.PageSize);
        var query = _dbContext.Set<T>().AsNoTracking();
        query = query.Filter(parameter);

        // Apply search term before counting
        query = query.SearchTerm(parameter.SearchTerm, parameter.GetSearchProps());

        response.Total = await query.CountAsync(ct);
        response.Items = await query.OrderBy(parameter.OrderBy)
                                    .Pagination(parameter.PageSize, parameter.PageNumber)
                                    .ToListAsync(ct);
        return response;
    }

    /// <summary>
    /// Retrieves a paged response of entities of type TModel based on the specified parameter settings.
    /// </summary>
    /// <typeparam name="TParam">The type of the parameter input, which must inherit from <see cref="BaseParameterRequest"/>.</typeparam>
    /// <typeparam name="TModel">The type of the model that the query result will be projected to, which must be a class.</typeparam>
    /// <param name="parameter">The parameter object containing pagination and ordering settings.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>A <see cref="PaginationResponse{T}"/> containing a paged list of entities of type TModel and total entity count.</returns>
    public virtual async Task<PaginationResponse<IReadOnlyList<TModel>>> GetModelPagedReponseAsync<TParam, TModel>(
        TParam parameter, CancellationToken ct = default)
        where TModel : class
        where TParam : BaseParameterRequest
    {
        var response = new PaginationResponse<IReadOnlyList<TModel>>(parameter.PageNumber, parameter.PageSize);
        var query = _dbContext.Set<T>().AsNoTracking();
        query = query.Filter(parameter);

        // Apply search term before counting
        query = query.SearchTerm(parameter.SearchTerm, parameter.GetSearchProps());

        response.Total = await query.CountAsync(ct);
        response.Items = await query.OrderBy(parameter.OrderBy)
                                    .Pagination(parameter.PageSize, parameter.PageNumber)
                                    .ProjectToType<TModel>()
                                    .ToListAsync(ct);
        return response;
    }

    /// <summary>
    /// Retrieves a paginated response containing a list of models mapped from entities, based on the provided query parameter.
    /// </summary>
    /// <typeparam name="TParam">The type of the parameter used to filter, order, and paginate the query, which must inherit from BaseParameterRequest.</typeparam>
    /// <typeparam name="TModel">The type of the model to map entities into, which must be a class.</typeparam>
    /// <param name="parameter">The query parameter containing pagination and optional sorting details.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a PaginationResponse object with the total count and a list of models for the current page.</returns>
    public virtual async Task<PaginationResponse<IReadOnlyList<TModel>>>
        GetModelSingleQueryPagedReponseAsync<TParam, TModel>(TParam parameter, CancellationToken ct = default)
        where TModel : class
        where TParam : BaseParameterRequest
    {
        var response = new PaginationResponse<IReadOnlyList<TModel>>(parameter.PageNumber, parameter.PageSize);
        var query = _dbContext.Set<T>().AsNoTracking();
        
        // Apply filters based on the parameter's filter rules
        query = query.Filter(parameter);

        // Apply search term filtering before counting (for accurate total count)
        query = query.SearchTerm(parameter.SearchTerm, parameter.GetSearchProps());

        // Count total items after filters/search applied 
        response.Total = await query.CountAsync(ct);
        
        // Retrieve paginated items (again) to assign to response object
        response.Items = await query.OrderBy(parameter.OrderBy)
                                    .Pagination(parameter.PageSize, parameter.PageNumber)
                                    .ProjectToType<TModel>()
                                    .AsSingleQuery()
                                    .ToListAsync(ct);
        return response;
    }

    /// <summary>
    /// Retrieves a paginated response based on the given query and parameter settings.
    /// This method applies filtering, search, ordering, and pagination rules
    /// before executing the query on the database. The result includes metadata
    /// such as total count, page number, and page size.
    /// </summary>
    /// <typeparam name="TParam">
    /// The request parameter type, which must inherit from <see cref="BaseParameterRequest"/>.
    /// Contains pagination, search, and ordering options.
    /// </typeparam>
    /// <typeparam name="TModel">
    /// The model type being queried.
    /// </typeparam>
    /// <param name="query">
    /// The base <see cref="IQueryable{TModel}"/> query on which operations will be applied.
    /// </param>
    /// <param name="parameter">
    /// Pagination and filtering parameters provided by the caller.
    /// Includes properties like PageNumber, PageSize, SearchTerm, and OrderBy.
    /// </param>
    /// <param name="ct">Cancellation token for async database operations.</param>
    /// <returns>
    /// A <see cref="PaginationResponse{T}"/> containing the paginated items
    /// and metadata such as total count and page information.
    /// </returns>
    public virtual async Task<PaginationResponse<IReadOnlyList<TModel>>> GetModelPagedReponseAsync<TParam, TModel>(
        IQueryable<TModel> query, TParam parameter, CancellationToken ct = default)
        where TModel : class
        where TParam : BaseParameterRequest
    {
        // Initialize the pagination response with page number and size
        var response = new PaginationResponse<IReadOnlyList<TModel>>(parameter.PageNumber, parameter.PageSize);

        // Apply filters based on the parameter's filter rules
        query = query.Filter(parameter);

        // Apply search term filtering before counting (for accurate total count)
        query = query.SearchTerm(parameter.SearchTerm, parameter.GetSearchProps());

        // Count total items after filters/search applied 
        response.Total = await query.CountAsync(ct);

        // Retrieve paginated items (again) to assign to response object
        response.Items = await query.OrderBy(parameter.OrderBy)
                                    .Pagination(parameter.PageSize, parameter.PageNumber)
                                    .ProjectToType<TModel>()
                                    .AsSingleQuery()
                                    .ToListAsync(ct);

        return response;
    }
    
    /// <summary>
    /// Retrieves a paged response of a specific model based on the provided query, parameters, and configuration.
    /// </summary>
    /// <typeparam name="TParam">The type of parameter used for filtering, searching, and pagination configuration, derived from <see cref="BaseParameterRequest"/>.</typeparam>
    /// <typeparam name="TModel">The type of the entity being queried, which must be a class.</typeparam>
    /// <typeparam name="TResponse">The type of the response returned after projection, which must be a class.</typeparam>
    /// <param name="query">The queryable collection of entities to filter, search, and paginate.</param>
    /// <param name="parameter">The parameter object containing filtering, search, ordering, and pagination details.</param>
    /// <param name="ct">The cancellation token to observe during the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous operation, containing a paginated response with a collection of the projected results.</returns>
    public virtual async Task<PaginationResponse<IReadOnlyList<TResponse>>> GetModelPagedReponseAsync<TParam, TModel, TResponse>(
        IQueryable<TModel> query, TParam parameter, CancellationToken ct = default)
        where TModel : class
        where TResponse : class
        where TParam : BaseParameterRequest
    {
        // Initialize the pagination response with page number and size
        var response = new PaginationResponse<IReadOnlyList<TResponse>>(parameter.PageNumber, parameter.PageSize);

        // Apply filters based on the parameter's filter rules
        query = query.Filter(parameter);

        // Apply search term filtering before counting (for accurate total count)
        query = query.SearchTerm(parameter.SearchTerm, parameter.GetSearchProps());

        // Count total items after filters/search applied 
        response.Total = await query.CountAsync(ct);

        // Retrieve paginated items to assign to response object
        response.Items = await query.OrderBy(parameter.OrderBy)
                                    .Pagination(parameter.PageSize, parameter.PageNumber)
                                    .ProjectToType<TResponse>()
                                    .AsSingleQuery()
                                    .ToListAsync(ct);

        return response;
    }
}