// Admin.Infrastructure/Persistence/SeedData/ProductTypeSeedData.cs
using Admin.Domain.Entities;

using Microsoft.EntityFrameworkCore;

using System.Text.Json;

namespace Admin.Infrastructure.Persistence.SeedData;

public static class ProductTypeSeedData
{
    public static async Task SeedProductTypesAsync(AdminDbContext context, ILogger logger)
    {
        if (await context.ProductTypes.AnyAsync())
        {
            return; // Already seeded
        }

        logger.LogInformation("Seeding product types");

        var clothingType = new ProductType(
            id: "clothing",
            name: "Clothing",
            description: "Apparel items including shirts, pants, dresses, etc.",
            icon: "checkroom");

        var clothingAttributes = new List<object>
        {
            new
            {
                Id = "size",
                Name = "Sizes",
                Type = "multiselect",
                IsRequired = true,
                Options = new List<object>
                {
                    new { Label = "XS", Value = "XS" },
                    new { Label = "S", Value = "S" },
                    new { Label = "M", Value = "M" },
                    new { Label = "L", Value = "L" },
                    new { Label = "XL", Value = "XL" },
                    new { Label = "XXL", Value = "XXL" }
                },
                DisplayOrder = 1,
                IsFilterable = true,
                IsComparable = false
            },
            // Add more attributes as needed
        };

        clothingType.UpdateAttributes(JsonSerializer.Serialize(clothingAttributes));

        // Add more product types
        var electronicsType = new ProductType(
            id: "electronics",
            name: "Electronics",
            description: "Electronic devices and accessories",
            icon: "devices");

        // Add electronics attributes...

        context.ProductTypes.Add(clothingType);
        context.ProductTypes.Add(electronicsType);

        await context.SaveChangesAsync();
        logger.LogInformation("Product types seeded successfully");
    }
}