using Admin.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Admin.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(x => x.Id);
        // This is important - tell EF Core to not generate IDs
        builder.Property(x => x.Id)
            .ValueGeneratedNever();
        builder.Ignore(x => x.DomainEvents);

        // Simple properties using field keyword
        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(x => x.Sku)
            .HasMaxLength(50)
            .IsRequired();

        // Money-related properties still using backing fields
        builder.Property<decimal>("_price")
            .HasColumnName("Price")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property<string>("_currency")
            .HasColumnName("Currency")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property<decimal?>("_compareAtPrice")
            .HasColumnName("CompareAtPrice")
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.Stock)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Visibility)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.HasOne(p => p.Category)
            .WithMany()
            .HasForeignKey(p => p.CategoryId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.SubCategory)
            .WithMany()
            .HasForeignKey(p => p.SubCategoryId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.ShortDescription)
            .HasMaxLength(500);

        builder.Property(x => x.Barcode)
            .HasMaxLength(100);

        builder.Property(x => x.LowStockThreshold)
            .HasColumnName("LowStockThreshold"); 

        // Collections
        builder.HasMany(x => x.Images)
            .WithOne(x => x.Product)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Variants)
            .WithOne(x => x.Product)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.OwnsMany(p => p.Attributes, attr =>
        {
            attr.WithOwner().HasForeignKey("ProductId");
            attr.Property<int>("Id").ValueGeneratedOnAdd();
            attr.HasKey("Id");
        });
        builder.OwnsOne(p => p.Dimensions, dimensions =>
        {
            dimensions.Property(d => d.Weight)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            dimensions.Property(d => d.Width)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            dimensions.Property(d => d.Height)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            dimensions.Property(d => d.Length)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            dimensions.Property(d => d.Unit)
                .HasMaxLength(10)
                .IsRequired();

            // Optional: customize the table column names if desired
            dimensions.Property(d => d.Weight).HasColumnName("DimensionWeight");
            dimensions.Property(d => d.Width).HasColumnName("DimensionWidth");
            dimensions.Property(d => d.Height).HasColumnName("DimensionHeight");
            dimensions.Property(d => d.Length).HasColumnName("DimensionLength");
            dimensions.Property(d => d.Unit).HasColumnName("DimensionUnit");
        });
        builder.OwnsOne(p => p.Seo, seo =>
        {
            seo.Property(s => s.Title)
                .HasMaxLength(200);

            seo.Property(s => s.Description)
                .HasMaxLength(500);

            seo.Property<List<string>>("_keywords")
                .HasColumnName("SeoKeywords")
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                    new ValueComparer<List<string>>(
                        (c1, c2) => c1.SequenceEqual(c2),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToList()))
                .HasMaxLength(1000);
        });

        builder.Property<List<string>>("_tags")
            .HasColumnName("Tags")
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                new ValueComparer<List<string>>(
                    (c1, c2) => c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()))
            .HasMaxLength(1000);


        // Audit properties handled by base configuration
        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .HasMaxLength(450);

        builder.Property(x => x.LastModifiedAt);

        builder.Property(x => x.LastModifiedBy)
            .HasMaxLength(450);

        builder.Property(x => x.IsArchived)
            .IsRequired()
            .HasDefaultValue(false);
    }
}
