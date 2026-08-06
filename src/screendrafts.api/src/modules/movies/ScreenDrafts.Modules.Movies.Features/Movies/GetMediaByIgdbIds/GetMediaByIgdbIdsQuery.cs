namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMediaByIgdbIds;

internal sealed record GetMediaByIgdbIdsQuery : IQuery<GetMediaByIgdbIdsResponse>
{
  public required IReadOnlyList<int> IgdbIds { get; init; }
}
