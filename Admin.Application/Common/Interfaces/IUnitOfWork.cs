namespace Admin.Application.Common.Interfaces;
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    Task<IDisposable> BeginTransactionAsync(CancellationToken cancellationToken);
}
