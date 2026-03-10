namespace ScreenDrafts.Modules.Movies.Features.Movies.SearchMovies;

internal sealed record SearchMoviesQuery : IQuery<SearchMoviesResponse>
{

  public required string DraftPartId { get; init; }
  public required string Query { get; init; }
  public int? Year { get; init; }
  public int Page { get; init; } = 1;
  public int PageSize { get; init; } = 20;
}
