namespace ScreenDrafts.Modules.Movies.Features.Movies.SearchMedia;

internal sealed record SearchMediaQuery : IQuery<SearchMediaResponse>
{
  public string Query { get; init; } = default!;
  public int? Year { get; init; }
  public int Page { get; init; } = 1;
  public int PageSize { get; init; } = 20;
}
