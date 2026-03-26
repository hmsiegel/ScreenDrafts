using ScreenDrafts.Common.Domain;

namespace ScreenDrafts.Modules.Integrations.PublicApi;

public sealed record MediaSearchApiResult
{
  public int? TmdbId { get; init; }
  public int? IgdbId { get; init; }
  public string Title { get; init; } = default!;
  public MediaType MediaType { get; init; } = default!;
  public string? Year { get; init; }
  public string? Poster { get; init; }
  public string? Overview { get; init; }
}
