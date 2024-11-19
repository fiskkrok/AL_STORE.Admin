namespace Admin.Infrastructure.Common.Exceptions;
public class StorageException : InfrastructureException
{
    public StorageException(string message, Exception innerException)
        : base(message, innerException) { }
}
