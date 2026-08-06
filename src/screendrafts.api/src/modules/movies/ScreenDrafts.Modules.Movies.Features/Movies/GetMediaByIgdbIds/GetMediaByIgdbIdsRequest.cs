namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMediaByIgdbIds;

internal sealed record GetMediaByIgdbIdsRequest
{
  [FromQuery(Name = "igdbIds")]
  public IReadOnlyList<int> IgdbIds { get; init; } = [];
}
