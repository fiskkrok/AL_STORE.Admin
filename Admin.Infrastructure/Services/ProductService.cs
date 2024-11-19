//using Admin.Application.Common.Interfaces;
//using Admin.Application.Services.Interfaces;
//using Admin.Domain.Entities;
//using Admin.Domain.Events;
//using Admin.Infrastructure.Persistence;
//using Admin.Infrastructure.Services.MessageBus;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Logging;
//using ProductStockUpdatedEvent = Admin.Application.Shared.Messages.ProductStockUpdatedEvent;

//namespace Admin.Application.Services;

//public class ProductService : IProductService
//{
//    private readonly AdminDbContext _context;
//    private readonly IFileStorageService _fileStorage;
//    private readonly IHubContext<NotificationHub> _hubContext;
//    private readonly ILogger<ProductService> _logger;
//    private readonly IMessageBusService _messageBus;

//    public ProductService(
//        AdminDbContext context,
//        IFileStorageService fileStorage,
//        IHubContext<NotificationHub> hubContext,
//        IMessageBusService messageBus,
//        ILogger<ProductService> logger)
//    {
//        _context = context;
//        _fileStorage = fileStorage;
//        _hubContext = hubContext;
//        _messageBus = messageBus;
//        _logger = logger;
//    }

//    public async Task<(IEnumerable<Product> Products, int TotalCount)> GetProductsAsync(
//        ProductFilterRequest filter,
//        CancellationToken cancellationToken = default)
//    {
//        var query = _context.Products
//            .Include(p => p.Category)
//            .Include(p => p.SubCategory)
//            .Include(p => p.Images)
//            .AsQueryable();

//        // Apply filters
//        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
//            query = query.Where(p =>
//                p.Name.Contains(filter.SearchTerm) ||
//                p.Description.Contains(filter.SearchTerm));

//        if (filter.CategoryId.HasValue) query = query.Where(p => p.CategoryId == filter.CategoryId);

//        if (filter.SubCategoryId.HasValue) query = query.Where(p => p.SubCategoryId == filter.SubCategoryId);

//        if (filter.MinPrice.HasValue) query = query.Where(p => p.Price >= filter.MinPrice);

//        if (filter.MaxPrice.HasValue) query = query.Where(p => p.Price <= filter.MaxPrice);

//        if (filter.InStock.HasValue) query = query.Where(p => filter.InStock.Value ? p.Stock > 0 : p.Stock == 0);

//        // Apply sorting
//        query = filter.SortBy?.ToLower() switch
//        {
//            "name" => filter.SortDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
//            "price" => filter.SortDescending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
//            "created" => filter.SortDescending
//                ? query.OrderByDescending(p => p.CreatedAt)
//                : query.OrderBy(p => p.CreatedAt),
//            _ => query.OrderByDescending(p => p.CreatedAt)
//        };

//        var totalCount = await query.CountAsync(cancellationToken);

//        // Apply pagination
//        var products = await query
//            .Skip((filter.PageNumber - 1) * filter.PageSize)
//            .Take(filter.PageSize)
//            .ToListAsync(cancellationToken);

//        return (products, totalCount);
//    }

//    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
//    {
//        return await _context.Products
//            .Include(p => p.Category)
//            .Include(p => p.SubCategory)
//            .Include(p => p.Images)
//            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
//    }

//    public async Task<Product> CreateAsync(
//        CreateProductRequest request,
//        CancellationToken cancellationToken = default)
//    {
//        var product = new Product
//        {
//            Id = Guid.NewGuid(),
//            Name = request.Name,
//            Description = request.Description,
//            Price = request.Price,
//            Stock = request.Stock,
//            CategoryId = request.CategoryId,
//            SubCategoryId = request.SubCategoryId,
//            CreatedAt = DateTime.UtcNow
//        };

//        _context.Products.Add(product);

//        // Handle image uploads
//        if (request.Images?.Any() == true)
//            foreach (var image in request.Images)
//            {
//                var imageUrl = await _fileStorage.UploadAsync(image, cancellationToken);
//                product.Images.Add(new ProductImage
//                {
//                    Id = Guid.NewGuid(),
//                    Url = imageUrl,
//                    FileName = image.FileName,
//                    Size = image.Length,
//                    CreatedAt = DateTime.UtcNow
//                });
//            }

//        await _context.SaveChangesAsync(cancellationToken);

//        // Publish message to RabbitMQ
//        await _messageBus.PublishAsync(new ProductCreatedMessage
//        {
//            ProductId = product.Id,
//            Name = product.Name,
//            Price = product.Price,
//            Stock = product.Stock,
//            Timestamp = DateTime.UtcNow
//        }, cancellationToken);

//        // Notify connected clients
//        await _hubContext.Clients.All.SendAsync(
//            "ProductCreated",
//            product,
//            cancellationToken);

//        return product;
//    }

//    public async Task<Product?> UpdateAsync(
//        Guid id,
//        CreateProductRequest request,
//        CancellationToken cancellationToken = default)
//    {
//        var product = await GetByIdAsync(id, cancellationToken);
//        if (product == null) return null;

//        product.Name = request.Name;
//        product.Description = request.Description;
//        product.Price = request.Price;
//        product.Stock = request.Stock;
//        product.CategoryId = request.CategoryId;
//        product.SubCategoryId = request.SubCategoryId;
//        product.UpdatedAt = DateTime.UtcNow;

//        // Handle image uploads
//        if (request.Images?.Any() == true)
//            foreach (var image in request.Images)
//            {
//                var imageUrl = await _fileStorage.UploadAsync(image, cancellationToken);
//                product.Images.Add(new ProductImage
//                {
//                    Id = Guid.NewGuid(),
//                    Url = imageUrl,
//                    FileName = image.FileName,
//                    Size = image.Length,
//                    CreatedAt = DateTime.UtcNow
//                });
//            }

//        await _context.SaveChangesAsync(cancellationToken);

//        // Publish message to RabbitMQ
//        await _messageBus.PublishAsync(new ProductUpdatedMessage
//        {
//            ProductId = product.Id,
//            Name = product.Name,
//            Price = product.Price,
//            Stock = product.Stock,
//            Timestamp = DateTime.UtcNow
//        }, cancellationToken);

//        // Notify connected clients
//        await _hubContext.Clients.All.SendAsync(
//            "ProductUpdated",
//            product,
//            cancellationToken);

//        return product;
//    }

//    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
//    {
//        var product = await GetByIdAsync(id, cancellationToken);
//        if (product == null) return false;

//        product.IsActive = false;
//        product.UpdatedAt = DateTime.UtcNow;

//        await _context.SaveChangesAsync(cancellationToken);

//        // Publish message to RabbitMQ
//        await _messageBus.PublishAsync(new ProductDeletedEvent
//        {
//            ProductId = product.Id,
//            Timestamp = DateTime.UtcNow
//        }, cancellationToken);

//        // Notify connected clients
//        await _hubContext.Clients.All.SendAsync(
//            "ProductDeleted",
//            id,
//            cancellationToken);

//        return true;
//    }

//    public async Task<bool> UpdateStockAsync(
//        Guid id,
//        int newStock,
//        CancellationToken cancellationToken = default)
//    {
//        var product = await GetByIdAsync(id, cancellationToken);
//        if (product == null) return false;

//        product.Stock = newStock;
//        product.UpdatedAt = DateTime.UtcNow;

//        await _context.SaveChangesAsync(cancellationToken);

//        // Publish message to RabbitMQ
//        await _messageBus.PublishAsync(new ProductStockUpdatedEvent
//        {
//            ProductId = product.Id,
//            NewStock = newStock,
//            Timestamp = DateTime.UtcNow
//        }, cancellationToken);

//        // Notify connected clients
//        await _hubContext.Clients.All.SendAsync(
//            "StockUpdated",
//            id,
//            newStock,
//            cancellationToken);

//        return true;
//    }
//}