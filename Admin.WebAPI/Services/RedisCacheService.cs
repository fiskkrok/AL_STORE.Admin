using Admin.Application.Common.Interfaces;
using Admin.Domain.Entities;
using StackExchange.Redis;

using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Admin.WebAPI.Services;

//RedisCacheService.cs
public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(30);

    public RedisCacheService(
        IConnectionMultiplexer redis,
        ILogger<RedisCacheService> logger)
    {
        _redis = redis;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            IncludeFields = true,
            // Remove this line to stop generating $id and $values
            // ReferenceHandler = ReferenceHandler.Preserve,
            MaxDepth = 32,
            Converters = { new ProductJsonConverter(), new CategoryJsonConverter() }
        };

        //TODO Option B (better long-term) - Use DTOs for caching:
        //Create a ProductCacheDto for serialization
        //    Use AutoMapper to convert between Product and ProductCacheDto
        //    Cache the DTOs instead of domain entities
        

        //This is a better approach because:


        //It separates your domain model from serialization concerns
        //It avoids circular references entirely
        //    It's more efficient for caching (only storing what you need)
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var db = _redis.GetDatabase();
            var value = await db.StringGetAsync(key);

            if (!value.HasValue)
            {
                _logger.LogDebug("Cache miss for key: {Key}", key);
                return null;
            }

            var result = JsonSerializer.Deserialize<T>(value.ToString(), _jsonOptions);
            _logger.LogDebug("Cache hit for key: {Key}", key);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache value for key {Key}", key);
            return null;
        }
    }
    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var db = _redis.GetDatabase();
            var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);

            await db.StringSetAsync(
                key,
                serializedValue,
                expiration ?? _defaultExpiration);

            _logger.LogDebug("Successfully cached value for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache value for key {Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync(key);
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
            var db = _redis.GetDatabase();
            var endpoints = _redis.GetEndPoints();
            var server = _redis.GetServer(endpoints.First());

            var keys = server.Keys(pattern: $"{prefixKey}*").ToArray();

            if (keys.Any())
                await db.KeyDeleteAsync(keys);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache keys with prefix {Prefix}", prefixKey);
        }
    }
}

public class CategoryJsonConverter : JsonConverter<Category>
{
    public override Category? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        using (JsonDocument document = JsonDocument.ParseValue(ref reader))
        {
            var root = document.RootElement;

            try
            {
                // Required properties for constructor
                var name = root.GetProperty("name").GetString() ?? "";
                var description = root.GetProperty("description").GetString() ?? "";

                // Create the category using the constructor
                var category = new Category(name, description);

                // Use reflection to set the Id since it's init-only
                if (root.TryGetProperty("id", out var idElement))
                {
                    var idField = typeof(Category)
                        .GetField("<Id>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
                    idField?.SetValue(category, idElement.GetGuid());
                }

                // Set ParentCategoryId if present
                if (root.TryGetProperty("parentCategoryId", out var parentIdElement))
                {
                    var parentIdProperty = typeof(Category)
                        .GetProperty("ParentCategoryId", BindingFlags.Public | BindingFlags.Instance);
                    parentIdProperty?.SetValue(category, parentIdElement.GetGuid());
                }

                return category;
            }
            catch (Exception ex)
            {
                throw new JsonException($"Error deserializing Category: {ex.Message}", ex);
            }
        }
    }

    public override void Write(Utf8JsonWriter writer, Category value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString("id", value.Id);
        writer.WriteString("name", value.Name);
        writer.WriteString("description", value.Description);

        if (value.ParentCategoryId.HasValue)
        {
            writer.WriteString("parentCategoryId", value.ParentCategoryId.Value);
        }

        writer.WriteEndObject();
    }
}
