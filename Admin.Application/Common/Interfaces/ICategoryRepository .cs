namespace Admin.Application.Common.Interfaces;
public interface ICategoryRepository : IRepository<Domain.Entities.Category>
{
    Task<IEnumerable<Domain.Entities.Category>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
