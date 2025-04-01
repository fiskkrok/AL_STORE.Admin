using System.Text.Json;
using System.Text.Json.Serialization;

using Admin.Application.Common.Interfaces;

using Polly;
using Polly.Retry;

using StackExchange.Redis;

namespace Admin.WebAPI.Services;

/// <summary>
/// Implementation of ICacheService using Redis with DTO-based caching
/// </summary>
public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(30);
    private readonly AsyncRetryPolicy _retryPolicy;

    public RedisCacheService(
        IConnectionMultiplexer redis,
        ILogger<RedisCacheService> logger)
    {
        _redis = redis;
        _logger = logger;

        // Standard JSON serialization options without custom converters for domain entities
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        // Create retry policy for Redis operations
        _retryPolicy = Policy
            .Handle<RedisConnectionException>()
            .Or<RedisTimeoutException>()
            .Or<RedisServerException>()
            .WaitAndRetryAsync(
                3, // retry count
                retryAttempt => TimeSpan.FromMilliseconds(Math.Pow(2, retryAttempt) * 100), // exponential backoff
                (ex, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(ex,
                        "Redis operation failed (Attempt {RetryCount}/3). Retrying in {RetryDelay}ms",
                        retryCount, timeSpan.TotalMilliseconds);
                });
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var db = _redis.GetDatabase();
                var value = await db.StringGetAsync(key);

                if (!value.HasValue)
                {
                    _logger.LogDebug("Cache miss for key: {Key}", key);
                    return null;
                }

                try
                {
                    var result = JsonSerializer.Deserialize<T>(value.ToString(), _jsonOptions);
                    _logger.LogDebug("Cache hit for key: {Key}", key);
                    return result;
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Error deserializing cache value for key {Key}", key);

                    // Remove corrupted data
                    await db.KeyDeleteAsync(key);
                    return null;
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache value for key {Key}", key);
            return null; // Ensure we never break functionality due to cache issues
        }
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default) where T : class
    {
        if (value == null) return;

        try
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                var db = _redis.GetDatabase();
                var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);

                await db.StringSetAsync(
                    key,
                    serializedValue,
                    expiration ?? _defaultExpiration);

                _logger.LogDebug("Successfully cached value for key: {Key}", key);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache value for key {Key}", key);
            // Continue execution - cache failures shouldn't break functionality
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                var db = _redis.GetDatabase();
                await db.KeyDeleteAsync(key);
                _logger.LogDebug("Removed cache key: {Key}", key);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache key {Key}", key);
        }
    }

    public async Task RemoveByPrefixAsync(string prefixKey, CancellationToken cancellationToken = default)
    {
        try
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                var db = _redis.GetDatabase();
                var endpoints = _redis.GetEndPoints();

                // In case of multiple endpoints, use the first server for SCAN
                var server = _redis.GetServer(endpoints.First());

                // Use SCAN for efficient pattern matching (use only when necessary)
                var keys = server.Keys(pattern: $"{prefixKey}*").ToArray();

                if (keys.Any())
                {
                    await db.KeyDeleteAsync(keys);
                    _logger.LogDebug("Removed {Count} cache keys with prefix {Prefix}", keys.Length, prefixKey);
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache keys with prefix {Prefix}", prefixKey);
        }
    }
}