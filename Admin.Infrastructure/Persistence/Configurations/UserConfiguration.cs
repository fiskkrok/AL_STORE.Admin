
using System.Text.Json;
using Admin.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Admin.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Ignore(x => x.DomainEvents);

        // Base properties
        builder.Property(x => x.Username)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.PasswordHash)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.LastLoginAt)
            .IsRequired(false);

        // Store roles and permissions as JSON
        builder.Property<List<string>>("_roles")
            .HasColumnName("Roles")
            .HasJsonConversion();

        builder.Property<List<string>>("_permissions")
            .HasColumnName("Permissions")
            .HasJsonConversion();

        // Indexes
        builder.HasIndex(x => x.Username)
            .IsUnique()
            .HasFilter("[IsActive] = 1");

        builder.HasIndex(x => x.Email)
            .IsUnique()
            .HasFilter("[IsActive] = 1");

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

// Extension method for JSON conversion
public static class PropertyBuilderExtensions
{
    public static PropertyBuilder<T> HasJsonConversion<T>(this PropertyBuilder<T> propertyBuilder)
    {
        return propertyBuilder
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, null as JsonSerializerOptions),
                v => System.Text.Json.JsonSerializer.Deserialize<T>(v, null as JsonSerializerOptions) ?? default!);
    }
}
