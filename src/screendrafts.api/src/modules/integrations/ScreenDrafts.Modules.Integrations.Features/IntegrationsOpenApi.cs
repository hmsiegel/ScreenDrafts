namespace ScreenDrafts.Modules.Integrations.Features;

internal static class IntegrationsOpenApi
{
  public static class Tags
  {
    public const string Movies = "Movies";
    public const string Zoom = "Zoom";
    public const string YouTube = "YouTube";
    public const string Games = "Games";
    public const string People = "People";
  }

  public static class Names
  {
    public const string Movies_Search = "OnlineMedia.Search";
    public const string Movies_Lookup = "OnlineMedia.Lookup";
    public const string Movies_Import = "OnlineMedia.Import";
    public const string Zoom_Webhook = "Zoom.Webhook";
    public const string YouTube_Search = "YouTube.Search";
    public const string Games_Search = "Games.Search";
    public const string People_Search = "People.Search";
    public const string People_GetFilmography = "People.GetFilmography";
  }

  public static class Permissions
  {
    public const string MoviesSearch = "media:search";
    public const string MoviesImport = "media:import";
  }
}
