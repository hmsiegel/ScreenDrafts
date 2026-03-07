namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMovieSummary;

internal sealed record GetMovieSummaryQuery : IQuery<GetMovieSummaryResponse>
{
  public required string ImdbId { get; init; }
}
