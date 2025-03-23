namespace Admin.Infrastructure.Exceptions;
public class ValidationException : AppException
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("Validation.Error", "One or more validation errors occurred")
    {
        Errors = errors;
    }
}
