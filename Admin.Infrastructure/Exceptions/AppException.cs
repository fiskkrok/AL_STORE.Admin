namespace Admin.Infrastructure.Exceptions;
public class AppException : Exception
{
    public string Code { get; }

    public AppException(string code, string message) : base(message)
    {
        Code = code;
    }

    public AppException(string code, string message, Exception innerException)
        : base(message, innerException)
    {
        Code = code;
    }
}
