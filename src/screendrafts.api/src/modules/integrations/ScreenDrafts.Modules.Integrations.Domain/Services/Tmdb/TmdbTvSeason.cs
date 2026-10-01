namespace ScreenDrafts.Modules.Integrations.Domain.Services.Tmdb;

public sealed record TmdbTvSeason
{
  public int SeasonNumber { get; init; }
  public string Name { get; init; } = string.Empty;
  public int EpisodeCount { get; init; }
  public string? AirDate { get; init; }
}
