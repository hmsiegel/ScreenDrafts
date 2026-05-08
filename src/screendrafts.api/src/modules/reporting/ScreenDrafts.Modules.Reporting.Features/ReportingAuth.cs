namespace ScreenDrafts.Modules.Reporting.Features;

internal static class ReportingAuth
{
  internal static class Roles
  {
    internal const string Admin = "Administrator";
    internal const string SuperAdmin = "SuperAdministrator";
  }

  internal static class ClaimTypes
  {
    internal const string Permission = "permission";
  }

  internal static class Permissions
  {
    internal const string SpotlightCreate = "spotlights:create";
    internal const string SpotlightUpdate = "spotlights:update";
    internal const string SpotlightRead = "spotlights:read";
    internal const string StatsRead = "stats:read";

    /// <summary>
    /// Grants access to Patreon-inclusive episode counts in GET /stats.
    /// Maps to the existing drafts:read-patreon permission.
    /// </summary>
    internal const string StatsReadPatreon = "drafts:read-patreon";
  }
}
