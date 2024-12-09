using Admin.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Admin.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {


        builder.HasKey(x => x.Id);
        builder.Ignore(x => x.DomainEvents);

        builder.Property<string>("_name")
            .HasColumnName("Name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property<string>("_description")
            .HasColumnName("Description")
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property<string>("_sku")
            .HasColumnName("Sku")
            .HasMaxLength(50)
            .IsRequired();

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

        builder.Property<int>("_stock")
            .HasColumnName("Stock")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Visibility)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.HasOne(x => x.Category)
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SubCategory)
            .WithMany()
            .HasForeignKey(x => x.SubCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure collections
        builder.HasMany(x => x.Images)
            .WithOne(x => x.Product)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Variants)
            .WithOne(x => x.Product)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure owned types if any
        builder.OwnsMany(p => p.Attributes, attr =>
        {
            attr.WithOwner().HasForeignKey("ProductId");
            attr.Property<int>("Id").ValueGeneratedOnAdd();
            attr.HasKey("Id");
        });

        // Configure value conversions for collections
        builder.Property<List<string>>("_tags")
            .HasColumnName("Tags")
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
            .HasMaxLength(1000);


        // Audit properties
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
