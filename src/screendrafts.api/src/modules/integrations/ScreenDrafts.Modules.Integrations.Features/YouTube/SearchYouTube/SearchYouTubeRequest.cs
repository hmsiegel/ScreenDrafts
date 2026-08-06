namespace ScreenDrafts.Modules.Integrations.Features.YouTube.SearchYouTube;

internal sealed record SearchYouTubeRequest
{
  [FromQuery(Name = "query")]
  public required string Query { get; init; }

  [FromQuery(Name = "page")]
  public int Page { get; init; } = 1;
}
