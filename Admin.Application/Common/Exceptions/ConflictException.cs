namespace Admin.Application.Common.Exceptions;
public class ConflictException : AppException
{
    public ConflictException(string message)
        : base("Conflict", message)
    {
    }
}