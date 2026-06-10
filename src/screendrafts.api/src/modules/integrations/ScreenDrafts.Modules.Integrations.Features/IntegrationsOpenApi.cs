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
    public const string Movies_Search = "OnlineMedia.Search";
    public const string Movies_Lookup = "OnlineMedia.Lookup";
    public const string Movies_Import = "OnlineMedia.Import";
    public const string Zoom_Webhook = "Zoom.Webhook";
  }

  public static class Permissions
  {
    public const string MoviesSearch = "media:search";
    public const string MoviesImport = "media:import";
  }
}
