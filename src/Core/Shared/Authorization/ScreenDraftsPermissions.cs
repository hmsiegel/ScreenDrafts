namespace ScreenDrafts.Shared.Authorization;

public static class ScreenDraftsAction
{
    public const string View = nameof(View);
    public const string Search = nameof(Search);
    public const string Create = nameof(Create);
    public const string Update = nameof(Update);
    public const string Delete = nameof(Delete);
    public const string Export = nameof(Export);
    public const string Generate = nameof(Generate);
    public const string Clean = nameof(Clean);
    public const string UpgradeSubscription = nameof(UpgradeSubscription);
}

public static class ScreenDraftsResource
{
    public const string Tenants = nameof(Tenants);
    public const string Dashboard = nameof(Dashboard);
    public const string Hangfire = nameof(Hangfire);
    public const string Users = nameof(Users);
    public const string UserRoles = nameof(UserRoles);
    public const string Roles = nameof(Roles);
    public const string RoleClaims = nameof(RoleClaims);
    public const string Products = nameof(Products);
    public const string Brands = nameof(Brands);
    public const string Drafts = nameof(Drafts);
}

public static class ScreenDraftsPermissions
{
    private static readonly ScreenDraftsPermission[] _all = new ScreenDraftsPermission[]
    {
        new("View Dashboard", ScreenDraftsAction.View, ScreenDraftsResource.Dashboard),
        new("View Hangfire", ScreenDraftsAction.View, ScreenDraftsResource.Hangfire),
        new("View Users", ScreenDraftsAction.View, ScreenDraftsResource.Users),
        new("Search Users", ScreenDraftsAction.Search, ScreenDraftsResource.Users),
        new("Create Users", ScreenDraftsAction.Create, ScreenDraftsResource.Users),
        new("Update Users", ScreenDraftsAction.Update, ScreenDraftsResource.Users),
        new("Delete Users", ScreenDraftsAction.Delete, ScreenDraftsResource.Users),
        new("Export Users", ScreenDraftsAction.Export, ScreenDraftsResource.Users),
        new("View UserRoles", ScreenDraftsAction.View, ScreenDraftsResource.UserRoles),
        new("Update UserRoles", ScreenDraftsAction.Update, ScreenDraftsResource.UserRoles),
        new("View Roles", ScreenDraftsAction.View, ScreenDraftsResource.Roles),
        new("Create Roles", ScreenDraftsAction.Create, ScreenDraftsResource.Roles),
        new("Update Roles", ScreenDraftsAction.Update, ScreenDraftsResource.Roles),
        new("Delete Roles", ScreenDraftsAction.Delete, ScreenDraftsResource.Roles),
        new("View RoleClaims", ScreenDraftsAction.View, ScreenDraftsResource.RoleClaims),
        new("Update RoleClaims", ScreenDraftsAction.Update, ScreenDraftsResource.RoleClaims),
        new("View Products", ScreenDraftsAction.View, ScreenDraftsResource.Products, IsBasic: true),
        new("Search Products", ScreenDraftsAction.Search, ScreenDraftsResource.Products, IsBasic: true),
        new("Create Products", ScreenDraftsAction.Create, ScreenDraftsResource.Products),
        new("Update Products", ScreenDraftsAction.Update, ScreenDraftsResource.Products),
        new("Delete Products", ScreenDraftsAction.Delete, ScreenDraftsResource.Products),
        new("Export Products", ScreenDraftsAction.Export, ScreenDraftsResource.Products),
        new("View Brands", ScreenDraftsAction.View, ScreenDraftsResource.Brands, IsBasic: true),
        new("Search Brands", ScreenDraftsAction.Search, ScreenDraftsResource.Brands, IsBasic: true),
        new("Create Brands", ScreenDraftsAction.Create, ScreenDraftsResource.Brands),
        new("Update Brands", ScreenDraftsAction.Update, ScreenDraftsResource.Brands),
        new("Delete Brands", ScreenDraftsAction.Delete, ScreenDraftsResource.Brands),
        new("Generate Brands", ScreenDraftsAction.Generate, ScreenDraftsResource.Brands),
        new("Clean Brands", ScreenDraftsAction.Clean, ScreenDraftsResource.Brands),
        new("Create Drafts", ScreenDraftsAction.Create, ScreenDraftsResource.Drafts),
        new("View Tenants", ScreenDraftsAction.View, ScreenDraftsResource.Tenants, IsRoot: true),
        new("Create Tenants", ScreenDraftsAction.Create, ScreenDraftsResource.Tenants, IsRoot: true),
        new("Update Tenants", ScreenDraftsAction.Update, ScreenDraftsResource.Tenants, IsRoot: true),
        new("Upgrade Tenant Subscription", ScreenDraftsAction.UpgradeSubscription, ScreenDraftsResource.Tenants, IsRoot: true)
    };

    public static IReadOnlyList<ScreenDraftsPermission> All { get; } = new ReadOnlyCollection<ScreenDraftsPermission>(_all);
    public static IReadOnlyList<ScreenDraftsPermission> Root { get; } = new ReadOnlyCollection<ScreenDraftsPermission>(_all.Where(p => p.IsRoot).ToArray());
    public static IReadOnlyList<ScreenDraftsPermission> Admin { get; } = new ReadOnlyCollection<ScreenDraftsPermission>(_all.Where(p => !p.IsRoot).ToArray());
    public static IReadOnlyList<ScreenDraftsPermission> Basic { get; } = new ReadOnlyCollection<ScreenDraftsPermission>(_all.Where(p => p.IsBasic).ToArray());
}

public record ScreenDraftsPermission(string Description, string Action, string Resource, bool IsBasic = false, bool IsRoot = false)
{
    public string Name => NameFor(Action, Resource);
    public static string NameFor(string action, string resource) => $"Permissions.{resource}.{action}";
}
