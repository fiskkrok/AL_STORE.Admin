using FluentValidation.Results;

namespace Admin.Application.Common.Exceptions;
public class ValidationException : Exception
{
    public ValidationException()
        : base("One or more validation failures have occurred.")
    {
        Errors = new Dictionary<string, string[]>();
    }
    public IDictionary<string, string[]> Errors { get; }
    public ValidationException(IEnumerable<ValidationFailure> failures)
        : this()
    {
        var failureGroups = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage);
        foreach (var failureGroup in failureGroups)
        {
            var property = failureGroup.Key;
            var errors = failureGroup.ToArray();
            Errors.Add(property, errors);
        }
    }
}
