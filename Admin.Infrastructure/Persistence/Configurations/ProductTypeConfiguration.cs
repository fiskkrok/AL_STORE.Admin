// Admin.Infrastructure/Persistence/Configurations/ProductTypeConfiguration.cs
using Admin.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Admin.Infrastructure.Persistence.Configurations;

public class ProductTypeConfiguration : IEntityTypeConfiguration<ProductType>
{
    public void Configure(EntityTypeBuilder<ProductType> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Ignore(x => x.DomainEvents);

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.Icon)
            .HasMaxLength(50);

        builder.Property(x => x.AttributesJson)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        // Audit properties
        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .HasMaxLength(450);

        builder.Property(x => x.LastModifiedAt);

        builder.Property(x => x.LastModifiedBy)
            .HasMaxLength(450);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
    }
}