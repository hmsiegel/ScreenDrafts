namespace ScreenDrafts.Modules.Integrations.Domain.Services.Tmdb;

public sealed record TmdbTvResult
{
  public int Id { get; init; }
  public string Name { get; init; } = string.Empty;
  public string? FirstAirDate { get; init; }
  public string? PosterPath { get; init; }
  public string? Overview { get; init; }
}
