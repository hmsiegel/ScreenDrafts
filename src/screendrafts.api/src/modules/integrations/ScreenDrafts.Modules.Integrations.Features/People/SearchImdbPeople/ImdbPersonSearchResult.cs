namespace ScreenDrafts.Modules.Integrations.Features.People.SearchImdbPeople;

// ── Response ──────────────────────────────────────────────────────────────────

internal sealed record ImdbPersonSearchResult
{
  public string ImdbId { get; init; } = string.Empty;
  public string Name { get; init; } = string.Empty;
  public string? Description { get; init; }
  public string? PhotoUrl { get; init; }
}
