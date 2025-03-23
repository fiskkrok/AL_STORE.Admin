using Admin.Application.Common.Interfaces;
using Admin.Domain.Entities;
using Admin.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Logging;

namespace Admin.Infrastructure.Persistence.Decorators;
public class CachingUserRepositoryDecorator : IUserRepository
{
    private readonly IUserRepository _inner;
    private readonly ICacheService _cache;
    private readonly ILogger _logger;
    private readonly TimeSpan _cacheExpiration;

    private const string UserIdKeyPrefix = "user:id";
    private const string UserNameKeyPrefix = "user:name";
    private const string UserEmailKeyPrefix = "user:email";

    public CachingUserRepositoryDecorator(
        IUserRepository inner,
        ICacheService cache,
        ILogger logger,
        TimeSpan cacheExpiration)
    {
        _inner = inner;
        _cache = cache;
        _logger = logger;
        _cacheExpiration = cacheExpiration;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{UserIdKeyPrefix}:{id}";

        try
        {
            var cached = await _cache.GetAsync<User>(cacheKey, cancellationToken);
            if (cached != null)
            {
                _logger.LogDebug("Cache hit for user with ID {Id}", id);
                return cached;
            }

            _logger.LogDebug("Cache miss for user with ID {Id}", id);
            var user = await _inner.GetByIdAsync(id, cancellationToken);

            if (user != null)
            {
                await _cache.SetAsync(cacheKey, user, _cacheExpiration, cancellationToken);
            }

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error accessing cache for user with ID {Id}, falling back to repository", id);
            return await _inner.GetByIdAsync(id, cancellationToken);
        }
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{UserNameKeyPrefix}:{username.ToLower()}";

        try
        {
            var cached = await _cache.GetAsync<User>(cacheKey, cancellationToken);
            if (cached != null)
            {
                _logger.LogDebug("Cache hit for username {Username}", username);
                return cached;
            }

            _logger.LogDebug("Cache miss for username {Username}", username);
            var user = await _inner.GetByUsernameAsync(username, cancellationToken);

            if (user != null)
            {
                await _cache.SetAsync(cacheKey, user, _cacheExpiration, cancellationToken);
            }

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error accessing cache for username {Username}, falling back to repository", username);
            return await _inner.GetByUsernameAsync(username, cancellationToken);
        }
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{UserEmailKeyPrefix}:{email.ToLower()}";

        try
        {
            var cached = await _cache.GetAsync<User>(cacheKey, cancellationToken);
            if (cached != null)
            {
                _logger.LogDebug("Cache hit for email {Email}", email);
                return cached;
            }

            _logger.LogDebug("Cache miss for email {Email}", email);
            var user = await _inner.GetByEmailAsync(email, cancellationToken);

            if (user != null)
            {
                await _cache.SetAsync(cacheKey, user, _cacheExpiration, cancellationToken);
            }

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error accessing cache for email {Email}, falling back to repository", email);
            return await _inner.GetByEmailAsync(email, cancellationToken);
        }
    }

    // These check methods don't benefit much from caching
    public Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default)
    {
        return _inner.UsernameExistsAsync(username, cancellationToken);
    }

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return _inner.EmailExistsAsync(email, cancellationToken);
    }

    public async Task AddAsync(User entity, CancellationToken cancellationToken = default)
    {
        await _inner.AddAsync(entity, cancellationToken);
        // No cache invalidation needed for new entity
    }

    public async Task UpdateAsync(User entity, CancellationToken cancellationToken = default)
    {
        await _inner.UpdateAsync(entity, cancellationToken);
        await InvalidateUserCacheAsync(entity, cancellationToken);
    }

    public async Task RemoveAsync(User entity, CancellationToken cancellationToken = default)
    {
        await _inner.RemoveAsync(entity, cancellationToken);
        await InvalidateUserCacheAsync(entity, cancellationToken);
    }

    private async Task InvalidateUserCacheAsync(User user, CancellationToken cancellationToken)
    {
        try
        {
            var keysToInvalidate = new List<string>
            {
                $"{UserIdKeyPrefix}:{user.Id}",
                $"{UserNameKeyPrefix}:{user.Username.ToLower()}",
                $"{UserEmailKeyPrefix}:{user.Email.ToLower()}"
            };

            foreach (var key in keysToInvalidate)
            {
                await _cache.RemoveAsync(key, cancellationToken);
                _logger.LogDebug("Invalidated cache key {Key}", key);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error invalidating cache for user {UserId}", user.Id);
        }
    }
}
