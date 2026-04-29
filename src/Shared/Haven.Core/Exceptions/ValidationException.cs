namespace Haven.Core.Exceptions;

/// <summary>
/// Represents errors that occur during validation.
/// </summary>
public class ValidationException : Exception
{
    public string ErrorCode { get; set; }

    /// <summary>
    /// Gets a list of error messages.
    /// </summary>
    public List<ErrorDetailDto> Errors { get; } = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class.
    /// This exception is thrown when one or more validation failures occur during request validation.
    /// </summary>
    /// <param name="failures">A collection of <see cref="ValidationFailure"/> that represent validation errors.</param>
    /// <param name="errorCode">A string representing the error code associated with the validation failure. Defaults to BAD_REQUEST.</param>
    public ValidationException(
        IEnumerable<ValidationFailure> failures,
        string errorCode = BAD_REQUEST)
        : base(GetExceptionMessage(failures))
    {
        ErrorCode = errorCode;
        // Iterate through each ValidationFailure object
        foreach (var failure in failures)
        {
            // Add the error message to the Errors list
            Errors.Add(new ErrorDetailDto()
            {
                Field = failure.PropertyName,
                Issue = failure.ErrorMessage
            });
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class.
    /// This exception is thrown when one or more validation failures occur.
    /// </summary>
    /// <param name="failures">A collection of <see cref="ValidationResult"/> representing the validation failures.</param>
    /// <param name="errorCode">A string representing the error code associated with the validation failure. Defaults to BAD_REQUEST.</param>
    public ValidationException(
        IEnumerable<ValidationResult> failures,
        string errorCode = BAD_REQUEST)
        : base(VALIDATION_FAILURES_HAVE_OCCURRED)
    {
        ErrorCode = errorCode;
        // Iterate through each ValidationFailure object
        foreach (var failure in failures)
        {
            var members = failure.MemberNames.Any() ? failure.MemberNames : [string.Empty];
            foreach (var member in members)
            {
                // Add the error message to the Errors list
                Errors.Add(new ErrorDetailDto
                {
                    Field = member,
                    Issue = failure.ErrorMessage
                });
            }
        }
    }

    /// <summary>
    /// Builds the exception message based on validation failures.
    /// If the list is empty, returns the default validation error message.
    /// </summary>
    private static string GetExceptionMessage(IEnumerable<ValidationFailure> failures)
    {
        var list = failures?.ToList() ?? [];

        if (list.Count == 0)
            return VALIDATION_FAILURES_HAVE_OCCURRED;

        // Join all failure messages into a single readable string
        return string.Join(ONE_LINE_SEPARATOR, list.Select(f => f.ErrorMessage));
    }
}