namespace ScreenDrafts.Modules.Integrations.Features.Movies.FetchMedia;

internal sealed record FetchMediaRequest
{
  public int MediaType { get; init; } = default!;
  public int? TmdbId { get; init; }
  public int? IgdbId { get; init; }
  public string? ImdbId { get; init; }
  public int? TvSeriesTmdbId { get; init; }
  public int? SeasonNumber { get; init; }
  public int? EpisodeNumber { get; init; }
}
