namespace ScreenDrafts.Modules.Movies.Features.Movies.SearchMedia;

internal sealed record SearchMediaResponse
{
  public PagedResult<MediaSearchResultResponse> Results { get; init; } = default!;
}
