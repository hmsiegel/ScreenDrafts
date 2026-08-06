namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMediaByExternalIds;

internal sealed record GetMediaByExternalIdsRequest
{
  [FromQuery(Name = "externalIds")]
  public IReadOnlyList<string> ExternalIds { get; init; } = [];
}
