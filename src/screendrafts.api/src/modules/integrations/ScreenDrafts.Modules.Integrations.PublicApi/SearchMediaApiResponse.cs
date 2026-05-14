namespace ScreenDrafts.Modules.Integrations.PublicApi;

public sealed record SearchMediaApiResponse
{
  public IReadOnlyList<MediaSearchApiResult> Results { get; init; } = [];
  public int TotalCount { get; init; }
  public int TotalPages { get; init; }
  public int Page { get; init; }
}
