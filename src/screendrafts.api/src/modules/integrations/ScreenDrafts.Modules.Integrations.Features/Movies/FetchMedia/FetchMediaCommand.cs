namespace ScreenDrafts.Modules.Integrations.Features.Movies.FetchMedia;

internal sealed record FetchMediaCommand : ICommand
{
  public MediaType MediaType { get; init; } = default!;
  public int? TmdbId { get; init; } = null!;
  public int? IgdbId { get; init; } = null!;
  public string? ImdbId { get; init; } = null!;
  public int? TvSeriesTmdbId { get; init; } = null!;
  public int? SeasonNumber { get; init; } = null!;
  public int? EpisodeNumber { get; init; } = null!;
} 
