using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Admin.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Admin.Infrastructure.Configuration;
public class StockItemConfiguration : IEntityTypeConfiguration<StockItem>
{
    public void Configure(EntityTypeBuilder<StockItem> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Ignore(x => x.DomainEvents);

        builder.Property(x => x.ProductId)
            .IsRequired();

        builder.Property<int>("_currentStock")
            .HasColumnName("CurrentStock")
            .IsRequired();

        builder.Property<int>("_reservedStock")
            .HasColumnName("ReservedStock")
            .IsRequired();

        builder.Property<int>("_lowStockThreshold")
            .HasColumnName("LowStockThreshold")
            .IsRequired();

        builder.Property<bool>("_trackInventory")
            .HasColumnName("TrackInventory")
            .IsRequired();

        // Computed properties
        builder.Property(x => x.AvailableStock)
            .HasComputedColumnSql("[CurrentStock] - [ReservedStock]");

        builder.Property(x => x.IsLowStock)
            .HasComputedColumnSql("CASE WHEN [CurrentStock] - [ReservedStock] <= [LowStockThreshold] THEN 1 ELSE 0 END");

        builder.Property(x => x.IsOutOfStock)
            .HasComputedColumnSql("CASE WHEN [CurrentStock] - [ReservedStock] <= 0 THEN 1 ELSE 0 END");

        // Relationships
        builder.HasMany<StockReservation>()
            .WithOne()
            .HasForeignKey(x => x.StockItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.ProductId)
            .IsUnique();

        builder.HasIndex(x => x.IsLowStock);
        builder.HasIndex(x => x.IsOutOfStock);
    }
}
