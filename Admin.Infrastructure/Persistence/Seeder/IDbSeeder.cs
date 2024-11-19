namespace Admin.Infrastructure.Persistence.Seeder;

public interface IDbSeeder
{
    Task SeedAsync();
}

public interface ICategorySeeder
{
    Task SeedAsync();
}

public interface IProductSeeder
{
    Task SeedAsync();
}