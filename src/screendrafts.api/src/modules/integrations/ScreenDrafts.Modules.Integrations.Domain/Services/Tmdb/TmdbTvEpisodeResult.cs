namespace ScreenDrafts.Modules.Integrations.Domain.Services.Tmdb;

public sealed record TmdbTvEpisodeResult
{
  public int Id { get; init; }
  public string Name { get; init; } = string.Empty;
  public string? AirDate { get; init; }
  public string? StillPath { get; init; }
  public string? Overview { get; init; }
  public int ShowId { get; init; }
  public int SeasonNumber { get; init; }
  public int EpisodeNumber { get; init; }
}
