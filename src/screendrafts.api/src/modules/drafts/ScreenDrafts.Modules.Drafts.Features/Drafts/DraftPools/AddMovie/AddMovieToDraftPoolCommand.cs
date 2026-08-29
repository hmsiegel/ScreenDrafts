namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftPools.AddMovie;

internal sealed record AddMovieToDraftPoolCommand : ICommand
{
  public required string PublicId { get; init; }

  public int TmdbId { get; init; }
  public required MediaType MediaType { get; init; }
  public int? TvSeriesTmdbId { get; init; }
  public int? SeasonNumber { get; init; }
  public int? EpisodeNumber { get; init; }
}
