namespace Admin.Application.Common.Exceptions;
public class DomainException : AppException
{
    public DomainException(string rule, string message)
        : base($"Domain.{rule}", message)
    {
    }
}
