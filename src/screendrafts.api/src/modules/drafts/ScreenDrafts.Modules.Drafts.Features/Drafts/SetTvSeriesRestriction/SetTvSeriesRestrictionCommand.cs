namespace ScreenDrafts.Modules.Drafts.Features.Drafts.SetTvSeriesRestriction;

internal sealed record SetTvSeriesRestrictionCommand : ICommand
{
  public required string PublicId { get; init; }
  public int? TvSeriesTmdbId { get; init; }
  public string? TvSeriesTitle { get; init; }
}
