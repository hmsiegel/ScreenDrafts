namespace ScreenDrafts.Modules.Movies.Features.Movies.SearchYouTubeMedia;

internal sealed record SearchYouTubeMediaQuery : IQuery<SearchYouTubeMediaResponse>
{
  public string Query { get; init; } = default!;
  public int Page { get; init; } = 1;
}
