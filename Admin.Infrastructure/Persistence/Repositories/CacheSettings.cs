namespace Admin.Infrastructure.Persistence.Repositories;

public class CacheSettings
{
    public TimeSpan DefaultExpiration { get; set; } = TimeSpan.FromMinutes(30);
    public TimeSpan ExtendedExpiration { get; set; } = TimeSpan.FromHours(24);
    public int MaxCacheSize { get; set; } = 1000;
}