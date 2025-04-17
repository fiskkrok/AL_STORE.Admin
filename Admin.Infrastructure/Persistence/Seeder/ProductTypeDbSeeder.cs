// Admin.Infrastructure/Persistence/SeedData/ProductTypeDbSeeder.cs

using System.Text.Json;

using Admin.Application.Services;
using Admin.Domain.Entities;

using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Admin.Infrastructure.Persistence.Seeder;

public class ProductTypeDbSeeder : IProductTypeSeeder
{
    private readonly AdminDbContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<ProductTypeDbSeeder> _logger;

    public ProductTypeDbSeeder(
        AdminDbContext context,
        IWebHostEnvironment env,
        ILogger<ProductTypeDbSeeder> logger
       )
    {
        _context = context;
        _env = env;
        _logger = logger;
    }
    private Guid CreateDeterministicGuid(string input)
    {
        // This creates the same Guid every time for the same input string
        using (var md5 = System.Security.Cryptography.MD5.Create())
        {
            byte[] hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
            return new Guid(hash);
        }
    }
    public async Task SeedAsync()
    {
        if (await _context.ProductTypes.AnyAsync())
        {
            return; // Already seeded
        }

        _logger.LogInformation("Seeding product types");

        // CLOTHING
        var clothingType = new ProductType(
            id: CreateDeterministicGuid("clothing").ToString(),
            name: "Clothing",
            description: "Apparel items including shirts, pants, dresses, etc.",
            icon: "checkroom");

        var clothingAttributes = new List<object>
    {
        new {
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
        new {
            Id = "color",
            Name = "Colors",
            Type = "color",
            IsRequired = true,
            DisplayOrder = 2,
            IsFilterable = true,
            IsComparable = false
        },
        new {
            Id = "material",
            Name = "Material",
            Type = "text",
            IsRequired = false,
            DisplayOrder = 3,
            IsFilterable = true,
            IsComparable = true
        },
        new {
            Id = "gender",
            Name = "Gender",
            Type = "select",
            IsRequired = true,
            Options = new List<object>
            {
                new { Label = "Men", Value = "men" },
                new { Label = "Women", Value = "women" },
                new { Label = "Unisex", Value = "unisex" },
                new { Label = "Boys", Value = "boys" },
                new { Label = "Girls", Value = "girls" }
            },
            DisplayOrder = 4,
            IsFilterable = true,
            IsComparable = false
        }
    };
        clothingType.UpdateAttributes(JsonSerializer.Serialize(clothingAttributes));
        _context.ProductTypes.Add(clothingType);

        // ELECTRONICS
        var electronicsType = new ProductType(
            id: CreateDeterministicGuid("electronics").ToString(),
            name: "Electronics",
            description: "Electronic devices and accessories",
            icon: "devices");

        var electronicsAttributes = new List<object>
    {
        new {
            Id = "brand",
            Name = "Brand",
            Type = "text",
            IsRequired = true,
            DisplayOrder = 1,
            IsFilterable = true,
            IsComparable = true
        },
        new {
            Id = "model",
            Name = "Model",
            Type = "text",
            IsRequired = true,
            DisplayOrder = 2,
            IsFilterable = true,
            IsComparable = true
        },
        new {
            Id = "warranty",
            Name = "Warranty Period (months)",
            Type = "number",
            IsRequired = false,
            DisplayOrder = 3,
            IsFilterable = true,
            IsComparable = true
        },
        new {
            Id = "connectivity",
            Name = "Connectivity",
            Type = "multiselect",
            IsRequired = false,
            Options = new List<object>
            {
                new { Label = "Bluetooth", Value = "bluetooth" },
                new { Label = "WiFi", Value = "wifi" },
                new { Label = "USB", Value = "usb" },
                new { Label = "HDMI", Value = "hdmi" },
                new { Label = "Ethernet", Value = "ethernet" }
            },
            DisplayOrder = 4,
            IsFilterable = true,
            IsComparable = true
        }
    };
        electronicsType.UpdateAttributes(JsonSerializer.Serialize(electronicsAttributes));
        _context.ProductTypes.Add(electronicsType);

        // BOOKS
        var booksType = new ProductType(
            id: CreateDeterministicGuid("books").ToString(),
            name: "Books",
            description: "Books, publications, and literature",
            icon: "menu_book");

        var booksAttributes = new List<object>
    {
        new {
            Id = "author",
            Name = "Author",
            Type = "text",
            IsRequired = true,
            DisplayOrder = 1,
            IsFilterable = true,
            IsComparable = false
        },
        new {
            Id = "isbn",
            Name = "ISBN",
            Type = "text",
            IsRequired = true,
            DisplayOrder = 2,
            IsFilterable = false,
            IsComparable = false,
            Validation = new {
                Pattern = @"^(?:ISBN(?:-1[03])?:? )?(?=[0-9X]{10}$|(?=(?:[0-9]+[- ]){3})[- 0-9X]{13}$|97[89][0-9]{10}$|(?=(?:[0-9]+[- ]){4})[- 0-9]{17}$)(?:97[89][- ]?)?[0-9]{1,5}[- ]?[0-9]+[- ]?[0-9]+[- ]?[0-9X]$",
                Message = "Please enter a valid ISBN"
            }
        },
        new {
            Id = "pages",
            Name = "Page Count",
            Type = "number",
            IsRequired = false,
            DisplayOrder = 3,
            IsFilterable = false,
            IsComparable = true
        },
        new {
            Id = "format",
            Name = "Format",
            Type = "select",
            IsRequired = true,
            Options = new List<object>
            {
                new { Label = "Hardcover", Value = "hardcover" },
                new { Label = "Paperback", Value = "paperback" },
                new { Label = "Audiobook", Value = "audiobook" },
                new { Label = "E-book", Value = "ebook" }
            },
            DisplayOrder = 4,
            IsFilterable = true,
            IsComparable = false
        }
    };
        booksType.UpdateAttributes(JsonSerializer.Serialize(booksAttributes));
        _context.ProductTypes.Add(booksType);

        // FURNITURE
        var furnitureType = new ProductType(
            id: CreateDeterministicGuid("furniture").ToString(),
            name: "Furniture",
            description: "Home and office furniture",
            icon: "chair");

        var furnitureAttributes = new List<object>
    {
        new {
            Id = "material",
            Name = "Material",
            Type = "multiselect",
            IsRequired = true,
            Options = new List<object>
            {
                new { Label = "Wood", Value = "wood" },
                new { Label = "Metal", Value = "metal" },
                new { Label = "Plastic", Value = "plastic" },
                new { Label = "Leather", Value = "leather" },
                new { Label = "Fabric", Value = "fabric" },
                new { Label = "Glass", Value = "glass" }
            },
            DisplayOrder = 1,
            IsFilterable = true,
            IsComparable = true
        },
        new {
            Id = "color",
            Name = "Color",
            Type = "color",
            IsRequired = true,
            DisplayOrder = 2,
            IsFilterable = true,
            IsComparable = false
        },
        new {
            Id = "style",
            Name = "Style",
            Type = "select",
            IsRequired = false,
            Options = new List<object>
            {
                new { Label = "Modern", Value = "modern" },
                new { Label = "Traditional", Value = "traditional" },
                new { Label = "Contemporary", Value = "contemporary" },
                new { Label = "Industrial", Value = "industrial" },
                new { Label = "Mid-Century", Value = "midcentury" },
                new { Label = "Scandinavian", Value = "scandinavian" }
            },
            DisplayOrder = 3,
            IsFilterable = true,
            IsComparable = false
        },
        new {
            Id = "assembly_required",
            Name = "Assembly Required",
            Type = "boolean",
            IsRequired = true,
            DisplayOrder = 4,
            IsFilterable = true,
            IsComparable = false
        }
    };
        furnitureType.UpdateAttributes(JsonSerializer.Serialize(furnitureAttributes));
        _context.ProductTypes.Add(furnitureType);

        // HOME & GARDEN
        var homeGardenType = new ProductType(
            id: CreateDeterministicGuid("home_garden").ToString(),
            name: "Home & Garden",
            description: "Home decor, gardening tools, and outdoor items",
            icon: "yard");

        var homeGardenAttributes = new List<object>
    {
        new {
            Id = "indoor_outdoor",
            Name = "Usage",
            Type = "select",
            IsRequired = true,
            Options = new List<object>
            {
                new { Label = "Indoor", Value = "indoor" },
                new { Label = "Outdoor", Value = "outdoor" },
                new { Label = "Both", Value = "both" }
            },
            DisplayOrder = 1,
            IsFilterable = true,
            IsComparable = false
        },
        new {
            Id = "seasonal",
            Name = "Seasonal",
            Type = "select",
            IsRequired = false,
            Options = new List<object>
            {
                new { Label = "All Seasons", Value = "all" },
                new { Label = "Spring", Value = "spring" },
                new { Label = "Summer", Value = "summer" },
                new { Label = "Fall", Value = "fall" },
                new { Label = "Winter", Value = "winter" }
            },
            DisplayOrder = 2,
            IsFilterable = true,
            IsComparable = false
        },
        new {
            Id = "material",
            Name = "Material",
            Type = "text",
            IsRequired = false,
            DisplayOrder = 3,
            IsFilterable = true,
            IsComparable = true
        }
    };
        homeGardenType.UpdateAttributes(JsonSerializer.Serialize(homeGardenAttributes));
        _context.ProductTypes.Add(homeGardenType);

        // BEAUTY & PERSONAL CARE
        var beautyType = new ProductType(
            id: CreateDeterministicGuid("beauty").ToString(),
            name: "Beauty & Personal Care",
            description: "Cosmetics, skincare, and personal care products",
            icon: "face");

        var beautyAttributes = new List<object>
    {
        new {
            Id = "skin_type",
            Name = "Skin Type",
            Type = "select",
            IsRequired = false,
            Options = new List<object>
            {
                new { Label = "All Skin Types", Value = "all" },
                new { Label = "Normal", Value = "normal" },
                new { Label = "Dry", Value = "dry" },
                new { Label = "Oily", Value = "oily" },
                new { Label = "Combination", Value = "combination" },
                new { Label = "Sensitive", Value = "sensitive" }
            },
            DisplayOrder = 1,
            IsFilterable = true,
            IsComparable = true
        },
        new {
            Id = "ingredients",
            Name = "Key Ingredients",
            Type = "text",
            IsRequired = false,
            DisplayOrder = 2,
            IsFilterable = false,
            IsComparable = true
        },
        new {
            Id = "vegan",
            Name = "Vegan",
            Type = "boolean",
            IsRequired = false,
            DisplayOrder = 3,
            IsFilterable = true,
            IsComparable = true
        },
        new {
            Id = "cruelty_free",
            Name = "Cruelty Free",
            Type = "boolean",
            IsRequired = false,
            DisplayOrder = 4,
            IsFilterable = true,
            IsComparable = true
        }
    };
        beautyType.UpdateAttributes(JsonSerializer.Serialize(beautyAttributes));
        _context.ProductTypes.Add(beautyType);

        // SPORTS & OUTDOORS
        var sportsType = new ProductType(
            id: CreateDeterministicGuid("sports").ToString(),
            name: "Sports & Outdoors",
            description: "Sporting goods, equipment, and outdoor recreation",
            icon: "sports_basketball");

        var sportsAttributes = new List<object>
    {
        new {
            Id = "sport_type",
            Name = "Sport/Activity",
            Type = "select",
            IsRequired = true,
            Options = new List<object>
            {
                new { Label = "Basketball", Value = "basketball" },
                new { Label = "Football", Value = "football" },
                new { Label = "Soccer", Value = "soccer" },
                new { Label = "Tennis", Value = "tennis" },
                new { Label = "Golf", Value = "golf" },
                new { Label = "Swimming", Value = "swimming" },
                new { Label = "Cycling", Value = "cycling" },
                new { Label = "Hiking", Value = "hiking" },
                new { Label = "Camping", Value = "camping" },
                new { Label = "Fishing", Value = "fishing" },
                new { Label = "Yoga", Value = "yoga" },
                new { Label = "Fitness", Value = "fitness" }
            },
            DisplayOrder = 1,
            IsFilterable = true,
            IsComparable = false
        },
        new {
            Id = "age_group",
            Name = "Age Group",
            Type = "select",
            IsRequired = false,
            Options = new List<object>
            {
                new { Label = "Adult", Value = "adult" },
                new { Label = "Youth", Value = "youth" },
                new { Label = "Children", Value = "children" }
            },
            DisplayOrder = 2,
            IsFilterable = true,
            IsComparable = false
        },
        new {
            Id = "skill_level",
            Name = "Skill Level",
            Type = "select",
            IsRequired = false,
            Options = new List<object>
            {
                new { Label = "Beginner", Value = "beginner" },
                new { Label = "Intermediate", Value = "intermediate" },
                new { Label = "Advanced", Value = "advanced" },
                new { Label = "Professional", Value = "professional" }
            },
            DisplayOrder = 3,
            IsFilterable = true,
            IsComparable = true
        },
        new {
            Id = "waterproof",
            Name = "Waterproof",
            Type = "boolean",
            IsRequired = false,
            DisplayOrder = 4,
            IsFilterable = true,
            IsComparable = true
        }
    };
        sportsType.UpdateAttributes(JsonSerializer.Serialize(sportsAttributes));
        _context.ProductTypes.Add(sportsType);

        // ACCESSORIES
        var accessoriesType = new ProductType(
            id: CreateDeterministicGuid("accessories").ToString(),
            name: "Accessories",
            description: "Watches, jewelry, and other fashion add-ons",
            icon: "watch");

        var accessoriesAttributes = new List<object>
    {
        new {
            Id = "brand",
            Name = "Brand",
            Type = "text",
            IsRequired = true,
            DisplayOrder = 1,
            IsFilterable = true,
            IsComparable = true
        },
        new {
            Id = "color",
            Name = "Color",
            Type = "color",
            IsRequired = false,
            DisplayOrder = 2,
            IsFilterable = true,
            IsComparable = false
        },
        new {
            Id = "material",
            Name = "Material",
            Type = "text",
            IsRequired = false,
            DisplayOrder = 3,
            IsFilterable = true,
            IsComparable = true
        }
    };
        accessoriesType.UpdateAttributes(JsonSerializer.Serialize(accessoriesAttributes));
        _context.ProductTypes.Add(accessoriesType);

        // OTHER
        var otherType = new ProductType(
            id: CreateDeterministicGuid("other").ToString(),
            name: "Other",
            description: "General products that do not fit a specific type",
            icon: "widgets");

        var otherAttributes = new List<object>
    {
        new {
            Id = "notes",
            Name = "Notes",
            Type = "text",
            IsRequired = false,
            DisplayOrder = 1,
            IsFilterable = false,
            IsComparable = false
        }
    };
        otherType.UpdateAttributes(JsonSerializer.Serialize(otherAttributes));
        _context.ProductTypes.Add(otherType);

        await _context.SaveChangesAsync();
        _logger.LogInformation("Product types seeded successfully");
    }

   
}