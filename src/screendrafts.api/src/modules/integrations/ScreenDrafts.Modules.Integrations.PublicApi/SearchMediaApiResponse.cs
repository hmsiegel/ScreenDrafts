namespace ScreenDrafts.Modules.Integrations.PublicApi;

public sealed record SearchMediaApiResponse
{
  public IReadOnlyList<MediaSearchApiResult> Results { get; init; } = [];
}
