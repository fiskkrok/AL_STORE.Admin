namespace Admin.Domain.Common;
/// <summary>
/// Base class for all auditable entities
/// </summary>
public abstract class AuditableEntity
{
    private readonly List<DomainEvent> _domainEvents = new();

    public Guid Id { get; private init; }
    public DateTime CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTime? LastModifiedAt { get; private set; }
    public string? LastModifiedBy { get; private set; }
    public bool IsActive { get; private set; } = true;

    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected AuditableEntity()
    {
        Id = Guid.NewGuid();
    }

    protected void AddDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void RemoveDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    protected void SetCreated(string? createdBy)
    {
        CreatedAt = DateTime.UtcNow;
        CreatedBy = createdBy;
    }

    protected void SetModified(string? modifiedBy)
    {
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;
    }

    protected void SetInactive(string? modifiedBy)
    {
        if (!IsActive) return;
        IsActive = false;
        SetModified(modifiedBy);
    }
}