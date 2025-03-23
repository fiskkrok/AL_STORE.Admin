namespace Admin.Infrastructure.Exceptions;
public class DomainException : AppException
{
    public DomainException(string rule, string message)
        : base($"Domain.{rule}", message)
    {
    }
}
