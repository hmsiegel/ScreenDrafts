namespace ScreenDrafts.Modules.Integrations.Domain.Services.Tmdb;

public sealed record TmdbSeasonEpisode
{
  public int Id { get; init; }
  public string Name { get; init; } = string.Empty;
  public int SeasonNumber { get; init; }
  public int EpisodeNumber { get; init; }
  public string? AirDate { get; init; }
  public string? Overview { get; init; }
  public string? StillPath { get; init; }
}
