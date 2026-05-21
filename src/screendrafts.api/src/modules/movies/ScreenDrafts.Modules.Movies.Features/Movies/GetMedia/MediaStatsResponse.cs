namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMedia;

public sealed record MediaStatsResponse(
  int TimesPlayed,
  int LandedOnBoard,
  int TimesVetoed,
  int TimesSaved
)
{
  public static readonly MediaStatsResponse Empty = new(0, 0, 0, 0);
}
