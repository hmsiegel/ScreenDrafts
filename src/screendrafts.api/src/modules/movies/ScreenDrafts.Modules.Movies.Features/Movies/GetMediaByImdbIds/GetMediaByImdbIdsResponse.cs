namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMediaByImdbIds;

internal sealed record GetMediaByImdbIdsResponse
{
  public IReadOnlyList<MediaImdbSummary> Items { get; init; } = [];
}
