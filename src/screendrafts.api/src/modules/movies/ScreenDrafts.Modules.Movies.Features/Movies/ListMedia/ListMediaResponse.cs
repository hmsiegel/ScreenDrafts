namespace ScreenDrafts.Modules.Movies.Features.Movies.ListMedia;

internal sealed record ListMediaResponse
{
  public required PagedResult<MediaListItemResponse> Result { get; init; }
}
