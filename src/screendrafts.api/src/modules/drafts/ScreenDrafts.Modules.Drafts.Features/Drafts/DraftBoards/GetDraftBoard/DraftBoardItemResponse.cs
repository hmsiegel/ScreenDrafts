namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftBoards.GetDraftBoard;

public sealed record DraftBoardItemResponse
{
  public int TmdbId { get; init; }
  public string? Notes { get; init; }
  public int? Priority { get; init; }
  public string? Title { get; init; }
  public string? Year { get; init; }
  public MediaType? MediaType { get; init; }
  public int? TvSeriesTmdbId { get; init; }
  public int? SeasonNumber { get; init; }
  public int? EpisodeNumber { get; init; }
  public string? TvSeriesTitle { get; init; }
}
