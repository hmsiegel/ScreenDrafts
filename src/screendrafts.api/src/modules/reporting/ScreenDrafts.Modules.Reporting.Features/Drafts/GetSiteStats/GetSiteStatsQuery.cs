namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetSiteStats;

internal sealed record GetSiteStatsQuery : IQuery<GetSiteStatsResponse>
{
  public bool IsPatreonMember { get; init; }
}
