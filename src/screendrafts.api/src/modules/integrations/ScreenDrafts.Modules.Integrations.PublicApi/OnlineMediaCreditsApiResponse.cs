namespace ScreenDrafts.Modules.Integrations.PublicApi;

/// <summary>
/// A title's people and companies as TMDb lists them. Actors keep TMDb's cast order, which is
/// the billing order.
/// </summary>
public sealed record OnlineMediaCreditsApiResponse
{
  public IReadOnlyList<OnlineCreditApiResult> Directors { get; init; } = [];
  public IReadOnlyList<OnlineCreditApiResult> Actors { get; init; } = [];
  public IReadOnlyList<OnlineCreditApiResult> Writers { get; init; } = [];
  public IReadOnlyList<OnlineCreditApiResult> Producers { get; init; } = [];
  public IReadOnlyList<OnlineCreditApiResult> ProductionCompanies { get; init; } = [];
}

public sealed record OnlineCreditApiResult
{
  public string Name { get; init; } = string.Empty;
  public string? ImdbId { get; init; }
  public int TmdbId { get; init; }
}
