namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMediaByIgdbIds;

internal sealed record GetMediaByIgdbIdsResponse
{
  public IReadOnlyList<MediaIgdbSummary> Items { get; init; } = [];
}
