namespace ScreenDrafts.Modules.Integrations.PublicApi;

public sealed record PersonProfileApiResponse
{
  public int TmdbId { get; init; }
  public string Name { get; init; } = string.Empty;
  public string? PhotoPath { get; init; }
}
