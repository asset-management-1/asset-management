namespace Be.Haven.Core.Interfaces.Repositories;

/// <summary>
/// Defines a generic repository interface for interacting with data storage,
/// supporting CRUD operations and various other query mechanisms for entity types.
/// </summary>
/// <typeparam name="T">The type of the entity managed by the repository. The type must be a class.</typeparam>
public interface IGenericRepository<T> where T : class
{
    /// <summary>
    /// Asynchronously adds a new entity of type <typeparamref name="T"/> to the data storage.
    /// </summary>
    /// <param name="entity">The entity of type <typeparamref name="T"/> to be added to the data storage.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the added entity of type <typeparamref name="T"/>.</returns>
    Task<T> AddAsync(T entity, CancellationToken ct = default);

    /// <summary>
    /// Adds a range of entities to the database context asynchronously (not yet saved).
    /// </summary>
    /// <param name="entities">The list of entities to be added to the database context.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);

    /// <summary>
    /// Asynchronously updates an existing entity in the database context with the new state provided.
    /// </summary>
    /// <param name="entity">The entity with updated values to be committed to the database.</param>
    /// <returns>A task representing the asynchronous operation, which does not produce any value.</returns>
    Task UpdateAsync(T entity);

    /// <summary>
    /// Updates a range of entities in the database by marking them as modified (not yet saved).
    /// </summary>
    /// <param name="entities">A list of entities to be updated in the database.</param>
    /// <returns>A task representing the asynchronous operation of updating the entities.</returns>
    Task UpdateRangeAsync(IEnumerable<T> entities);

    /// <summary>
    /// Asynchronously deletes the specified entity from the repository.
    /// </summary>
    /// <param name="entity">The entity to be deleted from the repository.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteAsync(T entity);

    /// <summary>
    /// Deletes a collection of entities from the database context asynchronously.
    /// </summary>
    /// <param name="entities">The list of entities to be deleted.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    Task DeleteRangeAsync(List<T> entities);
    
    /// <summary>
    /// Asynchronously retrieves an entity of type <typeparamref name="T"/> by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to be retrieved.</param>
    /// <
    /// returns>A task that represents the asynchronous operation. The task result contains the entity of type <typeparamref name="T"/> if found; otherwise, null.</returns>
    Task<T> GetByIdAsync(int id);

    /// <summary>
    /// Asynchronously retrieves the first entity that matches the specified predicate (read-only).
    /// </summary>
    /// <param name="predicate">The filter expression used to match an entity.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the first matching entity or null.</returns>
    Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);

    /// <summary>
    /// Retrieves all entities of type <typeparamref name="T"/> from the database (read-only).
    /// </summary>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing a read-only list of all entities.</returns>
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Determines asynchronously whether any entity matches the specified predicate.
    /// </summary>
    /// <param name="predicate">The filter expression to evaluate against entities.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing true if any entity matches; otherwise, false.</returns>
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);

    /// <summary>
    /// Counts asynchronously the number of entities that match the specified predicate (optional).
    /// </summary>
    /// <param name="predicate">An optional filter expression. If null, counts all entities.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the number of matching entities.</returns>
    Task<int> CountAsync(Expression<Func<T, bool>> predicate = null, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a list of entities that satisfy the given predicate (read-only).
    /// </summary>
    /// <param name="predicate">The filter expression to evaluate against entities.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing a list of matching entities.</returns>
    Task<IReadOnlyList<T>> GetListAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);

    /// <summary>
    /// Asynchronously retrieves a paged response of entities of type <typeparamref name="T"/> based on the specified parameters.
    /// </summary>
    /// <param name="parameter">The request parameters of type <typeparamref name="TParam"/> used to retrieve the paged data.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <typeparam name="TParam">The type of the request parameters, which must derive from <see cref="BaseParameterRequest"/>.</typeparam>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="PaginationResponse{T}"/> object containing a read-only list of entities of type <typeparamref name="T"/> and pagination details.</returns>
    Task<PaginationResponse<IReadOnlyList<T>>> GetPagedReponseAsync<TParam>(TParam parameter, CancellationToken ct = default)
        where TParam : BaseParameterRequest;

    /// <summary>
    /// Asynchronously retrieves a paginated specified response based on the provided parameters.
    /// </summary>
    /// <typeparam name="TParam">The type of the parameter used for the query. Must inherit from <see cref="BaseParameterRequest"/>.</typeparam>
    /// <typeparam name="TModel">The type of the model to be returned in the paginated response. Must be a class.</typeparam>
    /// <param name="parameter">The parameter object containing the query criteria for retrieving the paginated data.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains a <see cref="PaginationResponse{T}"/>
    /// with a read-only list of the specified model type.</returns>
    Task<PaginationResponse<IReadOnlyList<TModel>>> GetModelPagedReponseAsync<TParam, TModel>(TParam parameter, CancellationToken ct = default)
        where TModel : class
        where TParam : BaseParameterRequest;

    /// <summary>
    /// Asynchronously retrieves a paginated response containing a single query result set
    /// mapped to a model of type <typeparamref name="TModel"/> based on the specified parameters.
    /// </summary>
    /// <param name="parameter">The parameters of type <typeparamref name="TParam"/> used to filter and paginate the data.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <typeparam name="TParam">The type of the parameter object derived from <see cref="BaseParameterRequest"/>.</typeparam>
    /// <typeparam name="TModel">The type of the model that the query results will be mapped to. Must be a class.</typeparam>
    /// <returns>A task that represents the asynchronous operation. The task result contains
    /// a <see cref="PaginationResponse{T}"/> with a list of type <typeparamref name="TModel"/>
    /// generated from the result of the query.</returns>
    Task<PaginationResponse<IReadOnlyList<TModel>>> GetModelSingleQueryPagedReponseAsync<TParam, TModel>(
        TParam parameter, CancellationToken ct = default)
        where TModel : class
        where TParam : BaseParameterRequest;

    /// <summary>
    /// Asynchronously retrieves a paginated response of models of type <typeparamref name="TModel"/>
    /// based on the given query and parameter.
    /// </summary>
    /// <param name="query">An <see cref="IQueryable{TModel}"/> representing the source of models to be paginated.</param>
    /// <param name="parameter">The parameter of type <typeparamref name="TParam"/> that contains pagination and filtering information.</param>
    /// <param name="ct">A cancellation token to cancel the operation if required.</param>
    /// <typeparam name="TParam">The type of the parameter, which must derive from <see cref="BaseParameterRequest"/>.</typeparam>
    /// <typeparam name="TModel">The type of the model to be retrieved, which must be a class.</typeparam>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a
    /// <see cref="PaginationResponse{T}"/> containing a read-only list of models of type <typeparamref name="TModel"/>.
    /// </returns>
    Task<PaginationResponse<IReadOnlyList<TModel>>> GetModelPagedReponseAsync<TParam, TModel>(
        IQueryable<TModel> query,
        TParam parameter,
        CancellationToken ct = default)
        where TModel : class
        where TParam : BaseParameterRequest;
    
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
    Task<PaginationResponse<IReadOnlyList<TResponse>>> GetModelPagedReponseAsync<TParam, TModel, TResponse>(
        IQueryable<TModel> query, TParam parameter, CancellationToken ct = default)
        where TModel : class
        where TResponse : class
        where TParam : BaseParameterRequest;

    /// <summary>
    /// Asynchronously retrieves a list of projected results of type <typeparamref name="TResult"/> that satisfy the specified criteria.
    /// </summary>
    /// <param name="predicate">An expression that specifies the filtering criteria to apply to the entities of type <typeparamref name="T"/>.</param>
    /// <param name="selector">An expression that projects the entity of type <typeparamref name="T"/> into a result of type <typeparamref name="TResult"/>.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <typeparam name="TResult">The type of the result projected from the entity of type <typeparamref name="T"/>.</typeparam>
    /// <returns>A task that represents the asynchronous operation. The task result contains a read-only list of projected results of type <typeparamref name="TResult"/>.</returns>
    Task<IReadOnlyList<TResult>> GetListAsync<TResult>(
        Expression<Func<T, bool>> predicate,
        Expression<Func<T, TResult>> selector,
        CancellationToken ct = default);
}