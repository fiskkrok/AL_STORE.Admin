using System.Reflection;
using Admin.Application.Common.Interfaces;
using Admin.Domain.Common;
using Admin.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Admin.Infrastructure.Persistence;

public class AdminDbContext : DbContext, IApplicationDbContext, IUnitOfWork
{
    private readonly IDomainEventService _domainEventService;
    private readonly ICurrentUser _currentUser;

    public AdminDbContext(
        DbContextOptions<AdminDbContext> options,
        IDomainEventService domainEventService,
        ICurrentUser currentUser)
        : base(options)
    {
        _domainEventService = domainEventService;
        _currentUser = currentUser;
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<ProductType> ProductTypes => Set<ProductType>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<User> Users => Set<User>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        // Add this logging to see what EF Core is doing
        optionsBuilder.LogTo(Console.WriteLine);

        // Make sure we're not doing anything weird with tracking
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
    }
    
    public async Task<TEntity?> FindEntityAsync<TEntity>(Guid id, CancellationToken cancellationToken = default)
        where TEntity : class
    {
        return await Set<TEntity>().FindAsync([id], cancellationToken);
    }

    public void AttachEntity<TEntity>(TEntity entity) where TEntity : class
    {
        Set<TEntity>().Attach(entity);
    }

    public IQueryable<TEntity> QuerySet<TEntity>() where TEntity : class
    {
        return Set<TEntity>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Handle domain events
        var domainEvents = ChangeTracker.Entries<AuditableEntity>()
            .SelectMany(x => x.Entity.DomainEvents)
            .ToList();
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                // New entities
                entry.Property("CreatedAt").CurrentValue = DateTime.UtcNow;
                if (entry.Entity is Category category && category.ParentCategoryId.HasValue)
                {
                    // Ensure the parent is correctly tracked
                    var parentEntry = ChangeTracker.Entries<Category>()
                        .FirstOrDefault(e => e.Entity.Id == category.ParentCategoryId);

                    if (parentEntry != null)
                    {
                        parentEntry.State = EntityState.Unchanged;
                    }
                }
            }
        }
        var result = await base.SaveChangesAsync(cancellationToken);

        // Dispatch events after successful save
        foreach (var domainEvent in domainEvents)
        {
            await _domainEventService.PublishAsync(domainEvent, cancellationToken);
        }

        // Clear events
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            entry.Entity.ClearDomainEvents();
        }

        return result;
    }
}