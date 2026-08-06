namespace ScreenDrafts.Modules.Movies.Features.Movies.SearchGamesMedia;

internal sealed record SearchGamesMediaRequest
{
  [FromQuery(Name = "query")]
  public required string Query { get; init; }

  [FromQuery(Name = "page")]
  public int Page { get; init; } = 1;
}
