// Admin.Infrastructure/Persistence/DesignTimeDbContextFactory.cs
using Admin.Application.Common.Interfaces;
using Admin.Domain.Common;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Admin.Infrastructure.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AdminDbContext>
{
    public AdminDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.Development.json", optional: true)
            .Build();

        var builder = new DbContextOptionsBuilder<AdminDbContext>();
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        builder.UseSqlServer(connectionString);

        return new AdminDbContext(
            builder.Options,
            new NoOpDomainEventService(),  // Design-time implementation
            new NoOpCurrentUser());        // Design-time implementation
    }
}

// Design-time implementations
public class NoOpDomainEventService : IDomainEventService
{
    public Task PublishAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}

public class NoOpCurrentUser : ICurrentUser
{
    public string? Id => null;
    public string? Name => null;
    public IEnumerable<string> Roles => Array.Empty<string>();
}