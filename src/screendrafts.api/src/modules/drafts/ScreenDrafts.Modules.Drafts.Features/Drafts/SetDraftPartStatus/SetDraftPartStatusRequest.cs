namespace ScreenDrafts.Modules.Drafts.Features.Drafts.SetDraftPartStatus;

internal sealed record SetDraftPartStatusRequest
{
  [FromRoute(Name = "draftPublicId")]
  public required string DraftPublicId { get; init; }

  [FromRoute(Name = "partIndex")]
  public required int PartIndex { get; init; }

  public DraftPartStatusAction Action { get; init; }
}

