using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Admin.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Admin.Infrastructure.Persistence.Configurations;
public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Ignore(x => x.DomainEvents);

        builder.Property(x => x.ProductId)
            .IsRequired();

        builder.Property(x => x.Quantity)
            .IsRequired();

        // Money value object
        builder.OwnsOne(o => o.UnitPrice, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("UnitPrice")
                .HasColumnType("decimal(18,2)")
                .IsRequired();
            money.Property(m => m.Currency)
                .HasColumnName("UnitPriceCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });

        // Indexes
        builder.HasIndex(x => x.ProductId);
    }
}
