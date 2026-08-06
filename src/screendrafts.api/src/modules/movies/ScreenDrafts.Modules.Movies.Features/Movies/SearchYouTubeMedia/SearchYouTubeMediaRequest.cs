namespace ScreenDrafts.Modules.Movies.Features.Movies.SearchYouTubeMedia;

internal sealed record SearchYouTubeMediaRequest
{
  [FromQuery(Name = "query")]
  public required string Query { get; init; }

  [FromQuery(Name = "page")]
  public int Page { get; init; } = 1;
}
