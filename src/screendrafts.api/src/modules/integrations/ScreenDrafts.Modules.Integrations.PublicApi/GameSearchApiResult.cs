namespace ScreenDrafts.Modules.Integrations.PublicApi;

public sealed record GameSearchApiResult
{
  public int IgdbId { get; init; }
  public string Title { get; init; } = default!;
  public string? Year { get; init; }
  public string? Poster { get; init; }
}
