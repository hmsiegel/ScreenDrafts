namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SubDrafts.AdvanceSubDraft;

internal sealed record AdvanceSubDraftRequest
{
  [FromRoute(Name = "draftPartId")]
  public required string DraftPartPublicId { get; init; }

  [FromRoute(Name = "subDraftId")]
  public required string SubDraftPublicId { get; init; }
}

