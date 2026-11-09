namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftPools.AddMovie;

internal sealed record AddMovieToDraftPoolRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; init; } = default!;

  public int TmdbId { get; init; }
  public required int MediaType { get; init; }
  public int? TvSeriesTmdbId { get; init; }
  public int? SeasonNumber { get; init; }
  public int? EpisodeNumber { get; init; }
}
