namespace ScreenDrafts.Modules.Reporting.Features.Common;

internal static class ReportingCacheKeys
{
  public static string StaticCacheKey => "reporting:stats:static";
  public static string PublicEpisodeCacheKey => "reporting:stats:episodes:public";
  public static string PatreonEpisodeCacheKey => "reporting:stats:episodes:patreon";
  public static string SpotlightCacheKey => "reporting:spotlight:active";
}
