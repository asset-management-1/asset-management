namespace Be.Haven.Core.Exceptions;

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
        // Validation failures always return through Error.Details with a consistent issue-array shape.
        ErrorCode = string.IsNullOrWhiteSpace(errorCode) ? BAD_REQUEST : errorCode;
        foreach (var failure in failures)
        {
            Errors.Add(new ErrorDetailDto
            {
                Field = failure.PropertyName,
                Issue = new[] { failure.ErrorMessage }
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
        // Data-annotation validation is normalized to the same details shape as FluentValidation.
        ErrorCode = string.IsNullOrWhiteSpace(errorCode) ? BAD_REQUEST : errorCode;
        foreach (var failure in failures)
        {
            var members = failure.MemberNames.Any() ? failure.MemberNames : [string.Empty];
            foreach (var member in members)
            {
                Errors.Add(new ErrorDetailDto
                {
                    Field = member,
                    Issue = new[] { failure.ErrorMessage }
                });
            }
        }
    }

    /// <summary>
    /// Builds the exception message based on validation failures.
    /// If the list is empty, returns the default validation error message.
    /// </summary>
    /// <param name="failures">The validation failures used to build the exception message.</param>
    /// <returns>The combined validation message, or the default validation message when no failure exists.</returns>
    private static string GetExceptionMessage(IEnumerable<ValidationFailure> failures)
    {
        var list = failures?.ToList() ?? [];

        if (list.Count == 0)
            return VALIDATION_FAILURES_HAVE_OCCURRED;

        // Join all failure messages into one readable exception message for logs and clients.
        return string.Join(ONE_LINE_SEPARATOR, list.Select(f => f.ErrorMessage));
    }
}
