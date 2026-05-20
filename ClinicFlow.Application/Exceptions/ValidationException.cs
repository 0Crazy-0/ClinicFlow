using FluentValidation.Results;

namespace ClinicFlow.Application.Exceptions;

public class ValidationException : Exception
{
    /// <summary>
    /// Defines the default message used when no specific failures are provided.
    /// </summary>
    public const string DefaultErrorMessage = "One or more validation failures have occurred.";

    /// <summary>
    /// Gets the collection of validation failures grouped by property name.
    /// </summary>
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException()
        : base(DefaultErrorMessage)
    {
        Errors = new Dictionary<string, string[]>();
    }

    /// <param name="failures">The collection of validation failures to group.</param>
    public ValidationException(IEnumerable<ValidationFailure> failures)
        : this()
    {
        Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
    }
}
