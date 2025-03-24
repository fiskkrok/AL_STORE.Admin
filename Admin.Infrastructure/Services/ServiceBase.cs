using Admin.Application.Common.Exceptions;
using Microsoft.Extensions.Logging;

namespace Admin.Infrastructure.Services;
// Base service pattern for error handling
public abstract class ServiceBase
{
    protected virtual T ExecuteWithErrorHandling<T>(Func<T> action, string errorCode, string errorMessage)
    {
        try
        {
            return action();
        }
        catch (Exception ex) when (ex is not AppException)
        {
            throw new AppException(errorCode, errorMessage, ex);
        }
    }

    protected virtual async Task<T> ExecuteWithErrorHandlingAsync<T>(Func<Task<T>> action, string errorCode, string errorMessage)
    {
        try
        {
            return await action();
        }
        catch (Exception ex) when (ex is not AppException)
        {
            throw new AppException(errorCode, errorMessage, ex);
        }
    }

    protected virtual async Task ExecuteWithErrorHandlingAsync(Func<Task> action, string errorCode, string errorMessage)
    {
        try
        {
            await action();
        }
        catch (Exception ex) when (ex is not AppException)
        {
            throw new AppException(errorCode, errorMessage, ex);
        }
    }
}

// Logging decorator pattern for services
public class LoggingServiceDecorator<TService>
{
    private readonly TService _inner;
    private readonly ILogger _logger;

    public LoggingServiceDecorator(TService inner, ILogger logger)
    {
        _inner = inner;
        _logger = logger;
    }

    // Implement proxy methods for each service interface method
    // with added logging

    // Example:
    // public async Task<User> GetUserAsync(Guid userId)
    // {
    //     _logger.LogDebug("Getting user {UserId}", userId);
    //     var result = await _inner.GetUserAsync(userId);
    //     _logger.LogDebug("Retrieved user {UserId}: {UserFound}", userId, result != null);
    //     return result;
    // }
}
