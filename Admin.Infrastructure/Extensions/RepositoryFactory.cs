using Admin.Application.Common.Interfaces;
using Admin.Domain.Common;
using Admin.Infrastructure.Persistence;
using Admin.Infrastructure.Persistence.Decorators;
using Admin.Infrastructure.Persistence.Repositories;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Admin.Infrastructure.Extensions;
public class RepositoryFactory
{
    private readonly AdminDbContext _dbContext;
    private readonly ICacheService _cacheService;
    private readonly ILoggerFactory _loggerFactory;
    private readonly CacheSettings _cacheSettings;

    public RepositoryFactory(
        AdminDbContext dbContext,
        ICacheService cacheService,
        ILoggerFactory loggerFactory,
        IOptions<CacheSettings> cacheSettings)
    {
        _dbContext = dbContext;
        _cacheService = cacheService;
        _loggerFactory = loggerFactory;
        _cacheSettings = cacheSettings.Value;
    }

    // Generic repository creation
    public IRepository<TEntity> CreateRepository<TEntity>(bool enableCaching = true)
        where TEntity : AuditableEntity
    {
        var baseRepo = new BaseRepository<TEntity>(_dbContext);
        var logger = _loggerFactory.CreateLogger($"Repository<{typeof(TEntity).Name}>");

        // Always apply error handling
        IRepository<TEntity> repository = new ErrorHandlingRepositoryDecorator<TEntity>(
            baseRepo,
            _loggerFactory.CreateLogger<ErrorHandlingRepositoryDecorator<TEntity>>());

        // Apply logging
        repository = new LoggingRepositoryDecorator<TEntity>(
            repository,
            _loggerFactory.CreateLogger<LoggingRepositoryDecorator<TEntity>>());

        // Apply caching if requested
        if (enableCaching)
        {
            repository = new CachingRepositoryDecorator<TEntity>(
                repository,
                _cacheService,
                _loggerFactory.CreateLogger<CachingRepositoryDecorator<TEntity>>(),
                typeof(TEntity).Name.ToLowerInvariant(),
                _cacheSettings.DefaultExpiration);
        }

        return repository;
    }

    // Product repository
    public IProductRepository CreateProductRepository(bool enableCaching = true)
    {
        var baseRepo = new ProductRepository(_dbContext);
        var logger = _loggerFactory.CreateLogger<ProductRepository>();

        // Here we would add an error handling decorator if we had one for products
        IProductRepository repository = baseRepo;

        // Apply caching if requested
        if (enableCaching)
        {
            repository = new CachingProductRepositoryDecorator(
                repository,
                _cacheService,
                logger,
                _cacheSettings.DefaultExpiration);
        }

        return repository;
    }

    // Stock repository
    public IStockRepository CreateStockRepository(bool enableCaching = true)
    {
        var baseRepo = new StockRepository(_dbContext);
        var logger = _loggerFactory.CreateLogger<StockRepository>();

        // Here we would add an error handling decorator if we had one for stock
        IStockRepository repository = baseRepo;

        // Apply caching if requested
        if (enableCaching)
        {
            repository = new CachingStockRepositoryDecorator(
                repository,
                _cacheService,
                logger,
                _cacheSettings.DefaultExpiration);
        }

        return repository;
    }

    // Order repository
    public IOrderRepository CreateOrderRepository(bool enableCaching = true)
    {
        var baseRepo = new OrderRepository(_dbContext);
        var logger = _loggerFactory.CreateLogger<OrderRepository>();

        // Here we would add an error handling decorator if we had one for orders
        IOrderRepository repository = baseRepo;

        // Apply caching if requested
        if (enableCaching)
        {
            repository = new CachingOrderRepositoryDecorator(
                repository,
                _cacheService,
                logger,
                _cacheSettings.DefaultExpiration);
        }

        return repository;
    }

    // User repository
    public IUserRepository CreateUserRepository(bool enableCaching = true)
    {
        var baseRepo = new UserRepository(_dbContext);
        var logger = _loggerFactory.CreateLogger<UserRepository>();

        // Here we would add an error handling decorator if we had one for users
        IUserRepository repository = baseRepo;

        // Apply caching if requested
        if (enableCaching)
        {
            repository = new CachingUserRepositoryDecorator(
                repository,
                _cacheService,
                logger,
                _cacheSettings.DefaultExpiration);
        }

        return repository;
    }

    // Category repository
    public ICategoryRepository CreateCategoryRepository(bool enableCaching = true)
    {
        var baseRepo = new CategoryRepository(_dbContext);
        var logger = _loggerFactory.CreateLogger<CategoryRepository>();

        // Here we would add an error handling decorator if we had one for categories
        ICategoryRepository repository = baseRepo;

        // Apply caching if requested
        if (enableCaching)
        {
            repository = new CachingCategoryRepositoryDecorator(
                repository,
                _cacheService,
                logger,
                _cacheSettings.DefaultExpiration);
        }

        return repository;
    }
}
