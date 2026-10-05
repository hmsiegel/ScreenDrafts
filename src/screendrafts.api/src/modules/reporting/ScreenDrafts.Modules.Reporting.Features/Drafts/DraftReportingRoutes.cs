namespace ScreenDrafts.Modules.Reporting.Features.Drafts;

internal static class DraftReportingRoutes
{
  internal const string Stats = "/stats";
  internal const string Spotlight = "/spotlight";
  internal const string Spotlights = "/reporting/spotlights";
  internal const string ById = Spotlights + "/{publicId}";
  internal const string Activate = ById + "/activate";
  internal const string Deactivate = ById + "/deactivate";
  internal const string Candidates = Spotlights + "/candidates";
  internal const string Rotate = Spotlights + "/rotate";
  internal const string RecordBook = "/stats/record-book";
}
