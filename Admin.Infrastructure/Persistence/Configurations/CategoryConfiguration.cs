using Admin.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Admin.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Ignore(x => x.DomainEvents);

        // Basic properties
        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(x => x.Slug)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.SortOrder)
            .HasDefaultValue(0)
            .IsRequired();

        // SEO properties
        builder.Property(x => x.MetaTitle)
            .HasMaxLength(200);

        builder.Property(x => x.MetaDescription)
            .HasMaxLength(500);

        // Optional properties
        builder.Property(x => x.ImageUrl)
            .HasMaxLength(2000);

        // Hierarchical relationship - self-referencing
        builder.HasOne(x => x.ParentCategory)
            .WithMany(x => x.SubCategories)
            .HasForeignKey(x => x.ParentCategoryId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // Products relationship
        builder.HasMany(x => x.Products)
            .WithOne(x => x.Category)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Audit properties
        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .HasMaxLength(450);

        builder.Property(x => x.LastModifiedAt);

        builder.Property(x => x.LastModifiedBy)
            .HasMaxLength(450);

        // Indexing
        builder.HasIndex(x => x.Slug)
            .IsUnique()
            .HasFilter("[IsActive] = 1");

        builder.HasIndex(x => x.ParentCategoryId)
            .HasFilter("[IsActive] = 1");

        builder.HasIndex(x => x.IsActive);

        builder.HasIndex(x => x.SortOrder);

        // Instead of a computed column, we'll handle the path in the application layer
        // You can add a property to store the path if needed:
        builder.Property<string>("MaterializedPath")
            .HasMaxLength(4000)
            .IsRequired(false);
    }
}