namespace ScreenDrafts.Modules.Reporting.Features.Drafts.MarkDraftCompleted;

internal sealed record MarkDraftCompleteCommand : ICommand
{
  public Guid DraftId { get; init; }
}
