using Admin.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Admin.Infrastructure.Persistence.Configurations;
//ProductImageConfiguration.cs - Updated
public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Ignore(x => x.DomainEvents);

        builder.Property<string>("_url")
            .HasColumnName("Url")
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property<string>("_fileName")
            .HasColumnName("FileName")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property<string?>("_alt")
            .HasColumnName("Alt")
            .HasMaxLength(200);

        builder.Property<long>("_size")
            .HasColumnName("Size")
            .IsRequired();

        builder.Property<int>("_sortOrder")
            .HasColumnName("SortOrder")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property<bool>("_isPrimary")
            .HasColumnName("IsPrimary")
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.ProductId)
            .IsRequired();

        builder.HasOne(x => x.Product)
            .WithMany(x => x.Images)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Audit properties are handled by base configuration
        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .HasMaxLength(450);

        builder.Property(x => x.LastModifiedAt);

        builder.Property(x => x.LastModifiedBy)
            .HasMaxLength(450);
    }
}
