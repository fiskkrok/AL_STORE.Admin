using Admin.Domain.Entities;

using Microsoft.EntityFrameworkCore;

namespace Admin.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    // Entity DbSets
    DbSet<Product> Products { get; }
    DbSet<Category> Categories { get; }
    DbSet<ProductImage> ProductImages { get; }
    DbSet<ProductVariant> ProductVariants { get; }
    DbSet<ProductType> ProductTypes { get; }
    // Add other required DbSets

    // Generic entity methods
    Task<TEntity?> FindEntityAsync<TEntity>(Guid id, CancellationToken cancellationToken = default)
        where TEntity : class;

    // Add method for performing EF Core operations like attaching entities
    void AttachEntity<TEntity>(TEntity entity) where TEntity : class;

    // Method for querying
    IQueryable<TEntity> QuerySet<TEntity>() where TEntity : class;

    // Basic save method
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}