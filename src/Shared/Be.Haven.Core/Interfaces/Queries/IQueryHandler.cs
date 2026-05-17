namespace Be.Haven.Core.Interfaces.Queries;

/// <summary>
/// Interface for handling queries that return a <see cref="ResponseDto{TResponse}"/>.
/// </summary>
/// <typeparam name="TQuery">The type of the query.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public interface IQueryWithResponseHandler<TQuery, TResponse> : IRequestHandler<TQuery, ResponseDto<TResponse>>
    where TQuery : IQueryWithResponse<TResponse>
{
}

/// <summary>
/// Interface for handling queries with OData that return a <typeparamref name="TResponse"/>.
/// </summary>
/// <typeparam name="TQuery">The type of the query.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
}
