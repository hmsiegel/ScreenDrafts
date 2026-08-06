namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMediaByImdbIds;

internal sealed record GetMediaByImdbIdsRequest
{
  [FromQuery(Name = "imdbIds")]
  public IReadOnlyList<string> ImdbIds { get; init; } = [];
}
