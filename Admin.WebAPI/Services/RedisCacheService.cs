using Admin.Application.Common.Interfaces;
using Admin.Domain.Entities;
using Admin.Domain.Enums;

using StackExchange.Redis;
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
            // Add these options to handle complex types better
            IncludeFields = true,
            ReferenceHandler = ReferenceHandler.Preserve,
            // Add a custom converter for entities with constructors
            Converters = { new ProductJsonConverter() }
        };
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var db = _redis.GetDatabase();
            var value = await db.StringGetAsync(key);

            if (!value.HasValue)
                return null;

            return JsonSerializer.Deserialize<T>(value.ToString(), _jsonOptions);
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


public class ProductJsonConverter : JsonConverter<Product>
{
    public override Product? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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
                // Extract required properties for constructor
                var name = root.GetProperty("name").GetString() ?? "";
                var description = root.GetProperty("description").GetString() ?? "";

                // Handle price which might be stored as an object
                decimal price;
                if (root.TryGetProperty("price", out var priceElement))
                {
                    if (priceElement.ValueKind == JsonValueKind.Object)
                    {
                        price = priceElement.GetProperty("amount").GetDecimal();
                    }
                    else
                    {
                        price = priceElement.GetDecimal();
                    }
                }
                else
                {
                    price = 0m;
                }

                var currency = root.GetProperty("currency").GetString() ?? "USD";
                var sku = root.GetProperty("sku").GetString() ?? "";
                var stock = root.GetProperty("stock").GetInt32();
                var categoryId = root.GetProperty("categoryId").GetGuid();

                // Create product using constructor
                var product = new Product(
                    name,
                    description,
                    price,
                    currency,
                    sku,
                    stock,
                    categoryId
                );

                // Handle optional properties
                if (root.TryGetProperty("shortDescription", out var shortDesc))
                {
                    typeof(Product).GetProperty("ShortDescription")?.SetValue(product, shortDesc.GetString());
                }

                if (root.TryGetProperty("status", out var status) && Enum.TryParse<ProductStatus>(status.GetString(), out var statusEnum))
                {
                    typeof(Product).GetProperty("Status")?.SetValue(product, statusEnum);
                }

                if (root.TryGetProperty("visibility", out var visibility) && Enum.TryParse<ProductVisibility>(visibility.GetString(), out var visibilityEnum))
                {
                    typeof(Product).GetProperty("Visibility")?.SetValue(product, visibilityEnum);
                }

                if (root.TryGetProperty("subCategoryId", out var subCategoryId))
                {
                    typeof(Product).GetProperty("SubCategoryId")?.SetValue(product, subCategoryId.GetGuid());
                }

                // Handle collections if present
                if (root.TryGetProperty("images", out var images))
                {
                    foreach (var image in images.EnumerateArray())
                    {
                        var url = image.GetProperty("url").GetString();
                        var fileName = image.GetProperty("fileName").GetString();
                        var size = image.GetProperty("size").GetInt64();

                        if (url != null && fileName != null)
                        {
                            product.AddImage(url, fileName, size);
                        }
                    }
                }

                return product;
            }
            catch (Exception ex)
            {
                throw new JsonException($"Error deserializing Product: {ex.Message}", ex);
            }
        }
    }

    public override void Write(Utf8JsonWriter writer, Product value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        // Write basic properties
        writer.WriteString("id", value.Id);
        writer.WriteString("name", value.Name);
        writer.WriteString("description", value.Description);

        // Write price as an object
        writer.WriteStartObject("price");
        writer.WriteNumber("amount", value.Price.Amount);
        writer.WriteString("currency", value.Price.Currency);
        writer.WriteEndObject();

        writer.WriteString("currency", value.Price.Currency);
        writer.WriteString("sku", value.Sku);
        writer.WriteNumber("stock", value.Stock);
        writer.WriteString("categoryId", value.CategoryId);
        writer.WriteString("status", value.Status.ToString());
        writer.WriteString("visibility", value.Visibility.ToString());

        // Write optional properties
        if (value.ShortDescription != null)
            writer.WriteString("shortDescription", value.ShortDescription);

        if (value.SubCategoryId.HasValue)
            writer.WriteString("subCategoryId", value.SubCategoryId.Value);

        // Write collections
        writer.WriteStartArray("images");
        foreach (var image in value.Images)
        {
            writer.WriteStartObject();
            writer.WriteString("url", image.Url);
            writer.WriteString("fileName", image.FileName);
            writer.WriteNumber("size", image.Size);
            writer.WriteEndObject();
        }
        writer.WriteEndArray();

        writer.WriteEndObject();
    }
}
