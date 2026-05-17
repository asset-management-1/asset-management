namespace Be.Haven.Core.Interfaces.Queries;

/// <summary>
/// Interface for queries that return a <see cref="ResponseDto{TResponse}"/>.
/// </summary>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public interface IQueryWithResponse<TResponse> : IRequest<ResponseDto<TResponse>>
{
}

/// <summary>
/// Interface for queries with OData that return a <typeparamref name="TResponse"/>.
/// </summary>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public interface IQuery<out TResponse> : IRequest<TResponse>
{
}
