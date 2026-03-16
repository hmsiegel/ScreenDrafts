namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftPools.BulkAddMoviesToDraftPool;

internal sealed record BulkAddMoviesToDraftPoolCommand : ICommand<BulkAddMoviesResponse>
{
  public required string DraftId { get; init; }
  public required Stream CsvStream { get; init; }
}
