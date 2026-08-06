namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMediaByImdbIds;

internal sealed record GetMediaByImdbIdsQuery : IQuery<GetMediaByImdbIdsResponse>
{
  public required IReadOnlyList<string> ImdbIds { get; init; }
}
