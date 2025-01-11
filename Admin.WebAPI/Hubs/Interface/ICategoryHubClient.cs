using Admin.Application.Categories.DTOs;

namespace Admin.WebAPI.Hubs.Interface;

public interface ICategoryHubClient
{
    Task CategoryCreated(CategoryDto category);
    Task CategoryUpdated(CategoryDto category);
    Task CategoryDeleted(Guid categoryId);
}
