namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMediaByExternalIds;

internal sealed record GetMediaByExternalIdsResponse
{
  public IReadOnlyList<MediaExternalSummary> Items { get; init; } = [];
}
