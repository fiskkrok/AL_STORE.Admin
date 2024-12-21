using Admin.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Admin.Infrastructure.Persistence.Configurations;
public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Ignore(x => x.DomainEvents);

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

        builder.Property<int>("_stock")
            .HasColumnName("Stock")
            .IsRequired();

        builder.Property(x => x.ProductId)
            .IsRequired();

        builder.HasOne(x => x.Product)
            .WithMany(x => x.Variants)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure attributes as owned type collection
        builder.OwnsMany(x => x.Attributes, attr =>
        {
            attr.WithOwner().HasForeignKey("ProductVariantId");
            attr.Property<int>("Id").ValueGeneratedOnAdd();
            attr.HasKey("Id");

            attr.Property(x => x.Name)
                .HasMaxLength(50)
                .IsRequired();

            attr.Property(x => x.Value)
                .HasMaxLength(100)
                .IsRequired();

            attr.Property(x => x.Type)
                .HasMaxLength(50)
                .IsRequired();
        });

        // Audit properties
        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .HasMaxLength(450);

        builder.Property(x => x.LastModifiedAt);

        builder.Property(x => x.LastModifiedBy)
            .HasMaxLength(450);
    }
}