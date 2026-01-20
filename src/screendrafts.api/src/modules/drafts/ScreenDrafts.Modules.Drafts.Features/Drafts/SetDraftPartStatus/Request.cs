namespace ScreenDrafts.Modules.Drafts.Features.Drafts.SetDraftPartStatus;

internal sealed record Request
{
  [FromRoute(Name = "draftPublicId")]
  public required string DraftPublicId { get; init; }

  [FromRoute(Name = "partIndex")]
  public required int PartIndex { get; init; }

  public DraftPartStatusAction Action { get; init; }
}
