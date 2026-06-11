namespace ScreenDrafts.Modules.Reporting.Features.Drafts;

internal static class DraftReportingRoutes
{
  internal const string Stats = "/stats";
  internal const string Spotlight = "/spotlight";
  internal const string Spotlights = "/reporting/spotlights";
  internal const string ById = "/reporting/spotlights/{publicId}";
  internal const string Activate = "/reporting/spotlights/{publicId}/activate";
  internal const string Deactivate = "/reporting/spotlights/{publicId}/deactivate";
  internal const string Candidates = "/reporting/spotlights/candidates";
  internal const string Rotate = "/reporting/spotlights/rotate";
}
