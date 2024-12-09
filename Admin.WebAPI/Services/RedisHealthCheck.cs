using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace Admin.WebAPI.Services;

//RedisHealthCheck.cs
public class RedisHealthCheck : IHealthCheck
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisHealthCheck> _logger;

    public RedisHealthCheck(
        IConnectionMultiplexer redis,
        ILogger<RedisHealthCheck> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var result = await db.PingAsync();

            var response = TimeSpan.FromMilliseconds(result.TotalMilliseconds);
            var data = new Dictionary<string, object>
            {
                { "ResponseTime", response },
                { "Endpoints", string.Join(",", _redis.GetEndPoints().Select(e => e.ToString())) }
            };

            return response.TotalMilliseconds < 100
                ? HealthCheckResult.Healthy("Redis is healthy", data)
                : HealthCheckResult.Degraded("Redis response time is high", null, data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis health check failed");
            return HealthCheckResult.Unhealthy("Redis is unhealthy", ex);
        }
    }
}
