using System.Text.Json;
using System.Text.Json.Serialization;
using Admin.Domain.Enums;

namespace Admin.Domain.Entities;

public class ProductJsonConverter : JsonConverter<Product>
{
    public override Product? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        using (var document = JsonDocument.ParseValue(ref reader))
        {


            var root = document.RootElement;

            // DEBUG: Log the entire JSON to understand its structure
            var jsonString = JsonSerializer.Serialize(root, new JsonSerializerOptions { WriteIndented = true, ReferenceHandler = ReferenceHandler.Preserve });
            System.Diagnostics.Debug.WriteLine("===== INCOMING JSON DATA =====");
            System.Diagnostics.Debug.WriteLine(jsonString);
            System.Diagnostics.Debug.WriteLine("==============================");

            try
            {
                // Get the ID first with fallback
                Guid id;
                if (!root.TryGetProperty("id", out var idProperty) || !idProperty.TryGetGuid(out id))
                {
                    id = Guid.NewGuid();
                }

                // Get other properties with fallbacks
                var name = "";
                if (root.TryGetProperty("name", out var nameProperty))
                {
                    name = nameProperty.GetString() ?? "";
                }

                var description = "";
                if (root.TryGetProperty("description", out var descProperty))
                {
                    description = descProperty.GetString() ?? "";
                }

                decimal price = 0;
                if (root.TryGetProperty("price", out var priceProperty))
                {
                    if (priceProperty.ValueKind == JsonValueKind.Object && priceProperty.TryGetProperty("amount", out var amountProperty))
                    {
                        price = amountProperty.GetDecimal();
                    }
                    else
                    {
                        price = priceProperty.GetDecimal();
                    }
                }

                var currency = "USD";
                if (root.TryGetProperty("currency", out var currencyProperty))
                {
                    currency = currencyProperty.GetString() ?? "USD";
                }

                var sku = "";
                if (root.TryGetProperty("sku", out var skuProperty))
                {
                    sku = skuProperty.GetString() ?? "";
                }

                var stock = 0;
                if (root.TryGetProperty("stock", out var stockProperty))
                {
                    stock = stockProperty.GetInt32();
                }

                Guid categoryId;
                if (!root.TryGetProperty("categoryId", out var categoryIdProperty) || !categoryIdProperty.TryGetGuid(out categoryId))
                {
                    categoryId = Guid.Parse("FAB8190A-7BF6-4277-99B4-3BD000EDD45B"); // Default category
                }

                var product = new Product(
                    name: name,
                    description: description,
                    price: price,
                    currency: currency,
                    sku: sku,
                    stock: stock,
                    categoryId: categoryId,
                    id: id,
                    createdBy: null
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
                    // Check if images is an array or a single object
                    if (images.ValueKind == JsonValueKind.Array)
                    {
                        // Process array of images
                        foreach (var image in images.EnumerateArray())
                        {
                            string? url = null;
                            string? fileName = null;
                            long size = 0;

                            if (image.TryGetProperty("url", out var urlProperty))
                            {
                                url = urlProperty.GetString();
                            }

                            if (image.TryGetProperty("fileName", out var fileNameProperty))
                            {
                                fileName = fileNameProperty.GetString();
                            }

                            if (image.TryGetProperty("size", out var sizeProperty))
                            {
                                size = sizeProperty.GetInt64();
                            }

                            if (url != null && fileName != null)
                            {
                                product.AddImage(url, fileName, size);
                            }
                        }
                    }
                    else if (images.ValueKind == JsonValueKind.Object)
                    {
                        // Process single image object
                        string? url = null;
                        string? fileName = null;
                        long size = 0;

                        if (images.TryGetProperty("url", out var urlProperty))
                        {
                            url = urlProperty.GetString();
                        }

                        if (images.TryGetProperty("fileName", out var fileNameProperty))
                        {
                            fileName = fileNameProperty.GetString();
                        }

                        if (images.TryGetProperty("size", out var sizeProperty))
                        {
                            size = sizeProperty.GetInt64();
                        }

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
                // Log the exception with context
                System.Diagnostics.Debug.WriteLine($"Error while deserializing JSON: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"JSON data causing the error: {jsonString}");

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