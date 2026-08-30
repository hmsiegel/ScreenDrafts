namespace ScreenDrafts.Modules.Integrations.Features.Movies.SearchForTvShow;

internal sealed record SearchForTvShowRequest
{
  [FromQuery(Name = "query")]
  public string Query { get; init; } = string.Empty;

  [FromQuery(Name = "page")]
  public int Page { get; init; } = 1;
}
