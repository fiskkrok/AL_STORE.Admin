using Admin.Application.Common.CQRS;
using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Models;

using Microsoft.Extensions.Logging;

namespace Admin.Application.Products.Queries;

public record GetProductCountQuery : IQuery<int>;

public class GetProductCountQueryHandler : QueryHandler<GetProductCountQuery, int>
{
    private readonly IProductRepository _productRepository;

    public GetProductCountQueryHandler(
        IApplicationDbContext context,
        IProductRepository productRepository,
        ILogger<GetProductCountQueryHandler> logger)
        : base(context, logger)
    {
        _productRepository = productRepository;
    }

    public override async Task<Result<int>> Handle(GetProductCountQuery query, CancellationToken cancellationToken)
    {
        try
        {
            // Using the Query framework to count all products
            var count = await Task.FromResult(
                Query<Domain.Entities.Product>().Count()
            );

            return Result<int>.Success(count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting product count");
            return Result<int>.Failure(new Error("ProductCount.Failed", "Failed to get product count"));
        }
    }
}