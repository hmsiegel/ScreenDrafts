namespace ScreenDrafts.Modules.Drafts.Features.Categories;

internal static class CategoryRoutes
{
  public const string Category = "/categories";
  public const string ById = Category + "/{publicId:string}";
  public const string Restore = ById + "/restore";
}
