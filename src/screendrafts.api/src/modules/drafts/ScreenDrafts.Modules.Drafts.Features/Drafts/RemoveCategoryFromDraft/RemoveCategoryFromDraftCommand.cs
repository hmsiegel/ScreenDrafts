namespace ScreenDrafts.Modules.Drafts.Features.Drafts.RemoveCategoryFromDraft;

internal sealed record RemoveCategoryFromDraftCommand : ICommand
{
  public required string DraftId { get; init; }
  public required string CategoryId { get; init; }
}
