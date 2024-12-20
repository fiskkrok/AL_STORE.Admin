using System.Reflection;
using Admin.Application.Common.Interfaces;
using Admin.Domain.Common;
using Admin.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Admin.Infrastructure.Persistence;

public class AdminDbContext : DbContext, IUnitOfWork
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

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        // Add this logging to see what EF Core is doing
        optionsBuilder.LogTo(Console.WriteLine);

        // Make sure we're not doing anything weird with tracking
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
    }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(builder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Dispatch domain events before saving
        var domainEvents = ChangeTracker.Entries<AuditableEntity>()
            .SelectMany(x => x.Entity.DomainEvents)
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        // Dispatch events after successful save
        foreach (var domainEvent in domainEvents)
        {
            await _domainEventService.PublishAsync(domainEvent, cancellationToken);
        }

        return result;
    }
}