namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMediaByExternalIds;

internal sealed record GetMediaByExternalIdsQuery : IQuery<GetMediaByExternalIdsResponse>
{
  public required IReadOnlyList<string> ExternalIds { get; init; }
}
