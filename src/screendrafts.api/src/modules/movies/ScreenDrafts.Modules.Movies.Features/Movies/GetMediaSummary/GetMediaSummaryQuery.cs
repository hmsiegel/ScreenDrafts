namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMediaSummary;

internal sealed record GetMediaSummaryQuery : IQuery<GetMediaSummaryResponse>
{
  public required string PublicId { get; init; }
}
