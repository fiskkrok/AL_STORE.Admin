namespace Admin.Domain.Constants;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string User = "User";

    public static readonly IReadOnlyDictionary<string, string[]> RolePermissions = new Dictionary<string, string[]>
    {
        {
            Admin, [
                Permissions.Users.Manage,
                Permissions.Products.Manage,
                Permissions.Orders.Manage,
                Permissions.Categories.Manage
            ]
        },
        {
            Manager, [
                Permissions.Products.Manage,
                Permissions.Orders.Manage,
                Permissions.Categories.View
            ]
        },
        {
            User, [
                Permissions.Products.View,
                Permissions.Orders.View,
                Permissions.Categories.View
            ]
        }
    };
}

public static class Permissions
{
    public static class Users
    {
        public const string View = "Users.View";
        public const string Create = "Users.Create";
        public const string Edit = "Users.Edit";
        public const string Delete = "Users.Delete";
        public const string Manage = "Users.Manage";
    }

    public static class Products
    {
        public const string View = "Products.View";
        public const string Create = "Products.Create";
        public const string Edit = "Products.Edit";
        public const string Delete = "Products.Delete";
        public const string Manage = "Products.Manage";
    }

    public static class Orders
    {
        public const string View = "Orders.View";
        public const string Create = "Orders.Create";
        public const string Edit = "Orders.Edit";
        public const string Delete = "Orders.Delete";
        public const string Manage = "Orders.Manage";
    }

    public static class Categories
    {
        public const string View = "Categories.View";
        public const string Create = "Categories.Create";
        public const string Edit = "Categories.Edit";
        public const string Delete = "Categories.Delete";
        public const string Manage = "Categories.Manage";
    }
}



