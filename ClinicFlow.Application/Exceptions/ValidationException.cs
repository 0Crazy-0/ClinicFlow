using FluentValidation.Results;

namespace ClinicFlow.Application.Exceptions;

public class ValidationException : Exception
{
    public const string DefaultErrorMessage = "One or more validation failures have occurred.";

    public IDictionary<string, string[]> Errors { get; }

    public ValidationException()
        : base(DefaultErrorMessage)
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IEnumerable<ValidationFailure> failures)
        : this()
    {
        Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
    }
}
