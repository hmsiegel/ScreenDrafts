namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftBoards.BulkAddMoviesToDraftBoard;

internal sealed record BulkAddMoviesToDraftBoardCommand : ICommand<BulkAddMoviesResponse>
{
  public required string DraftId { get; init; }
  public required string UserPublicId { get; init; }
  public required Stream CsvStream { get; init; }
}
