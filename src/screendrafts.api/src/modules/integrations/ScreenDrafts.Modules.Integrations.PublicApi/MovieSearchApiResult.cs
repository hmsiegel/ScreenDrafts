namespace ScreenDrafts.Modules.Integrations.PublicApi;

public sealed record MovieSearchApiResult
{
  public int TmdbId { get; init; }
  public string Title { get; init; } = default!;
  public string? Year { get; init; }
  public string? Poster { get; init; }
  public string? Overview { get; init; }
}
