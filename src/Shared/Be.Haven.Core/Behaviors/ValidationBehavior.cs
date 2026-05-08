namespace Be.Haven.Core.Behaviors;

/// <summary>
/// Represents the validation behavior in a pipeline for processing requests and responses.
/// Ensures that the request data adheres to the specified validation rules before being processed further.
/// </summary>
/// <typeparam name="TRequest">The type of request being validated.</typeparam>
/// <typeparam name="TResponse">The type of response expected from processing the request.</typeparam>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    /// <summary>
    /// Represents the validation behavior in a pipeline for processing requests and responses,
    /// ensuring the request data meets specified validation rules before proceeding.
    /// </summary>
    /// <typeparam name="TRequest">The type of request being validated.</typeparam>
    /// <typeparam name="TResponse">The type of response expected from processing the request.</typeparam>
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    /// <summary>
    /// Handles the validation behavior in the pipeline for the specified request and response types.
    /// </summary>
    /// <param name="message">The request object of type <typeparamref name="TRequest"/> to validate.</param>
    /// <param name="next">The next delegate in the pipeline, which processes the request if validation passes.</param>
    /// <param name="cancellationToken">The token to observe while waiting for task completion.</param>
    /// <returns>The response object of type <typeparamref name="TResponse"/> if the validation is successful.</returns>
    /// <exception cref="Exceptions.ValidationException">Thrown when validation failures occur and the request contains invalid data.</exception>
    public async ValueTask<TResponse> Handle(
        TRequest message,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(message);

            var validationResults = await Task.WhenAll(
                _validators.Select(
                    v => v.ValidateAsync(context, cancellationToken)));

            var failures = validationResults
                .SelectMany(r => r.Errors)
                    .Where(f => f != null).ToList();

            if (failures.Count != 0)
            {
                var firstError = failures[0];
                throw new ValidationException(failures, firstError.ErrorCode);
            }    
        }
        return await next(message, cancellationToken);
    }
}
