using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Admin.Application.Common.CQRS;

public abstract class CommandHandler<TCommand, TResult> : IRequestHandler<TCommand, Result<TResult>>
    where TCommand : ICommand<TResult>
{
    protected readonly IApplicationDbContext Context;
    protected readonly ILogger Logger;

    protected CommandHandler(IApplicationDbContext context, ILogger logger)
    {
        Context = context;
        Logger = logger;
    }

    public abstract Task<Result<TResult>> Handle(TCommand command, CancellationToken cancellationToken);

    // Helper methods using IApplicationDbContext
    protected async Task<bool> ExistsAsync<TEntity>(Guid id, CancellationToken cancellationToken)
        where TEntity : class
    {
        return await Context.FindEntityAsync<TEntity>(id, cancellationToken) != null;
    }

    // Add other helper methods that work with IApplicationDbContext
    protected IQueryable<TEntity> Query<TEntity>() where TEntity : class
    {
        return Context.QuerySet<TEntity>();
    }
}