using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Models;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Admin.Application.Common.CQRS;

public abstract class QueryHandler<TQuery, TResult> : IRequestHandler<TQuery, Result<TResult>>
    where TQuery : IQuery<TResult>
{
    protected readonly IApplicationDbContext Context;
    protected readonly ILogger Logger;

    protected QueryHandler(IApplicationDbContext context, ILogger logger)
    {
        Context = context;
        Logger = logger;
    }

    public abstract Task<Result<TResult>> Handle(TQuery query, CancellationToken cancellationToken);

    // Helper method to execute optimized read queries
    protected IQueryable<TEntity> Query<TEntity>() where TEntity : class
    {
        return Context.QuerySet<TEntity>();
    }
}