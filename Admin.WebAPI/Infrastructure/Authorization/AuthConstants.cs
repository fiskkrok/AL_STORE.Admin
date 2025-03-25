namespace Admin.WebAPI.Infrastructure.Authorization;

public static class AuthConstants
{
    // Authentication Schemes
    public const string JwtBearerScheme = "Bearer";
    public const string ApiKeyScheme = "ApiKey";

    // Roles
    public const string SystemAdministratorRole = "SystemAdministrator";
    public const string ProductManagerRole = "ProductManager";
    public const string InventoryManagerRole = "InventoryManager";
    public const string CustomerServiceRole = "CustomerService";

    // Scopes
    public const string FullApiAccess = "api.full";
    public const string ProductsCreate = "products.create";
    public const string ProductsRead = "products.read";
    public const string ProductsUpdate = "products.update";
    public const string ProductsDelete = "products.delete";
    public const string CategoriesRead = "categories.read";
    public const string CategoriesManage = "categories.manage";

    // Policy Names
    public const string IsAdminPolicy = "IsAdmin";
    public const string CanManageProductsPolicy = "CanManageProducts";
    public const string CanReadProductsPolicy = "CanReadProducts";
    public const string CanManageCategoriesPolicy = "CanManageCategories";
    public const string CanReadCategoriesPolicy = "CanReadCategories";
    public const string CanManageOrdersPolicy = "CanManageOrders";
    public const string CanReadOrdersPolicy = "CanReadOrders";
}