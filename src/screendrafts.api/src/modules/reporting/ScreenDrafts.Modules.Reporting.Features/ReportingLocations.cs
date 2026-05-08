namespace ScreenDrafts.Modules.Reporting.Features;

internal static class ReportingLocations
{
  public static string ById(Guid spotlightId) => $"/reporting/spotlights/{spotlightId}";
}
