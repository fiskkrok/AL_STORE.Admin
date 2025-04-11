// Admin.Application/Products/Commands/UpsertProductTypeCommand.cs
using System.Text.Json;

using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Models;
using Admin.Application.Products.DTOs;
using Admin.Domain.Entities;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Admin.Application.Products.Commands;

public record UpsertProductTypeCommand : IRequest<Result<string>>
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Icon { get; init; }
    public List<ProductTypeAttributeDto> Attributes { get; init; } = new();
}

public class UpsertProductTypeCommandHandler : IRequestHandler<UpsertProductTypeCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly ICacheService _cacheService;
    private readonly ILogger<UpsertProductTypeCommandHandler> _logger;

    public UpsertProductTypeCommandHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        ICacheService cacheService,
        ILogger<UpsertProductTypeCommandHandler> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(
        UpsertProductTypeCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            ProductType productType;
            var isNew = string.IsNullOrEmpty(request.Id) || request.Id == "new";
            var id = isNew ? Guid.NewGuid().ToString() : request.Id;

            if (isNew)
            {
                // Create new
                productType = new ProductType(id, request.Name, request.Description, request.Icon);
                productType.UpdateAttributes(JsonSerializer.Serialize(request.Attributes));
                _context.ProductTypes.Add(productType);
            }
            else
            {
                // Update existing
                productType = await _context.ProductTypes
                    .FirstOrDefaultAsync(p => p.Id == new Guid(id), cancellationToken);

                if (productType == null)
                {
                    return Result<string>.Failure(
                        new Error("ProductType.NotFound", $"Product type with ID {id} was not found"));
                }

                productType.Update(request.Name, request.Description, request.Icon);
                productType.UpdateAttributes(JsonSerializer.Serialize(request.Attributes));
            }

            await _context.SaveChangesAsync(cancellationToken);

            // Invalidate cache
            await _cacheService.RemoveAsync($"product:type:{id}", cancellationToken);
            await _cacheService.RemoveAsync("product:types:all", cancellationToken);

            return Result<string>.Success(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving product type");
            return Result<string>.Failure(
                new Error("ProductType.SaveFailed", "Failed to save product type"));
        }
    }
}