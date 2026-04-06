namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SubDrafts.AdvanceSubDraft;

internal sealed record AdvanceSubDraftCommand : ICommand
{
  public required string DraftPartPublicId { get; init; }
  public required string SubDraftPublicId { get; init; }
}

