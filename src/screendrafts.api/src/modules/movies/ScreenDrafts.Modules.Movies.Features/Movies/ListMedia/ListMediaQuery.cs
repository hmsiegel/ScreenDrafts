namespace ScreenDrafts.Modules.Movies.Features.Movies.ListMedia;

internal sealed record ListMediaQuery : IQuery<ListMediaResponse>
{
  public int Page { get; init; } = 1;
  public int PageSize { get; init; } = 24;
  public string? Search { get; init; }
  public int? MediaType { get; init; }
  public string? Year { get; init; }
  public string? Sort { get; init; }
}
