namespace ScreenDrafts.Modules.Movies.Features.Movies.SearchMedia;

internal sealed record SearchMediaQuery : IQuery<SearchMediaResponse>
{

  public required string DraftPartId { get; init; }
  public required string Query { get; init; }
  public int? Year { get; init; }
  public int Page { get; init; } = 1;
  public int PageSize { get; init; } = 20;
}
