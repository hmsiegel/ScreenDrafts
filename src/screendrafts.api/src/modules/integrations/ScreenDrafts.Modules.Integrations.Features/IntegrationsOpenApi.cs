namespace ScreenDrafts.Modules.Integrations.Features;

internal static class IntegrationsOpenApi
{
  public static class Tags
  {
    public const string Movies = "Movies";
    public const string Zoom = "Zoom";
  }

  public static class Names
  {
    public const string Movies_Search = "Movies.Search";
    public const string Zoom_Webhook = "Zoom.Webhook";
  }

  public static class Permissions
  {
    public const string MoviesSearch = "movies:search";
  }
}
