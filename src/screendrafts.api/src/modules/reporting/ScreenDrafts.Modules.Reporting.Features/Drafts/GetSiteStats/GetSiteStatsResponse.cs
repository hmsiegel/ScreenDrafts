namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetSiteStats;

internal sealed record GetSiteStatsResponse
{
  public int EpisodesProduced { get; init; }
  public int FilmsDrafted { get; init; }
  public int GuestGMs { get; init; }
  public int VetoesDeployed { get; init; }
  public int Legends { get; init; }
}
