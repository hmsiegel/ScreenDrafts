namespace ScreenDrafts.Modules.Reporting.Features;

internal static class ReportingOpenApi
{
  internal static class Tags
  {
    internal const string Stats = "Stats";
    internal const string Spotlight = "Spotlight";
  }

  internal static class Names
  {
    internal const string Stats_GetSiteStats = "Stats:GetSiteStats";
    internal const string Spotlight_GetActive = "Spotlight:GetActive";
    internal const string Spotlight_Create = "Spotlight:Create";
    internal const string Spotlight_Update = "Spotlight:Update";
    internal const string Spotlight_GetById = "Spotlight:GetById";
    internal const string Spotlight_GetSpotlights = "Spotlight:GetSpotlights";
    internal const string Spotlight_Activate = "Spotlight:Activate";
    internal const string Spotlight_Deactivate = "Spotlight:Deactivate";
    internal const string Spotlight_SearchCandidates = "Spotlight:SearchCandidates";
    internal const string Spotlight_Delete = "Spotlight:Delete";
    internal const string Spotlight_RotateSpotlight = "Spotlight:RotateSpotlight";
  }
}
