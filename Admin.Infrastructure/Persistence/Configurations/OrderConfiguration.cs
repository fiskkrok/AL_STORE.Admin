using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Admin.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Admin.Infrastructure.Persistence.Configurations;
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Ignore(x => x.DomainEvents);

        // Basic properties
        builder.Property(x => x.OrderNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.CustomerId)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        // Money-value objects
        builder.OwnsOne(o => o.ShippingCost, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("ShippingCost")
                .HasColumnType("decimal(18,2)")
                .IsRequired();
            money.Property(m => m.Currency)
                .HasColumnName("ShippingCostCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.OwnsOne(o => o.Tax, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("Tax")
                .HasColumnType("decimal(18,2)")
                .IsRequired();
            money.Property(m => m.Currency)
                .HasColumnName("TaxCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });

        // Address value objects
        builder.OwnsOne(o => o.ShippingAddress, address =>
        {
            address.Property(a => a.Street).HasMaxLength(200).IsRequired();
            address.Property(a => a.City).HasMaxLength(100).IsRequired();
            address.Property(a => a.State).HasMaxLength(100).IsRequired();
            address.Property(a => a.Country).HasMaxLength(100).IsRequired();
            address.Property(a => a.PostalCode).HasMaxLength(20).IsRequired();
        });

        builder.OwnsOne(o => o.BillingAddress, address =>
        {
            address.Property(a => a.Street).HasMaxLength(200).IsRequired();
            address.Property(a => a.City).HasMaxLength(100).IsRequired();
            address.Property(a => a.State).HasMaxLength(100).IsRequired();
            address.Property(a => a.Country).HasMaxLength(100).IsRequired();
            address.Property(a => a.PostalCode).HasMaxLength(20).IsRequired();
        });

        // Shipping info value object
        builder.OwnsOne(o => o.ShippingInfo, shipping =>
        {
            shipping.Property(s => s.Carrier).HasMaxLength(100);
            shipping.Property(s => s.TrackingNumber).HasMaxLength(100);
            shipping.Property(s => s.EstimatedDeliveryDate);
            shipping.Property(s => s.ActualDeliveryDate);
        });

        // Payment value object
        builder.OwnsOne(o => o.Payment, payment =>
        {
            payment.Property(p => p.TransactionId).HasMaxLength(100);
            payment.Property(p => p.Method).HasConversion<string>().HasMaxLength(50);
            payment.Property(p => p.Status).HasConversion<string>().HasMaxLength(50);
            payment.Property(p => p.ProcessedAt);

            payment.OwnsOne(p => p.Amount, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("PaymentAmount")
                    .HasColumnType("decimal(18,2)");
                money.Property(m => m.Currency)
                    .HasColumnName("PaymentCurrency")
                    .HasMaxLength(3);
            });
        });

        // Other properties
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.Property(x => x.CancellationReason).HasMaxLength(500);

        // Indexes
        builder.HasIndex(x => x.OrderNumber).IsUnique();
        builder.HasIndex(x => x.CustomerId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.CreatedAt);
    }
}
