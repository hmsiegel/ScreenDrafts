namespace ScreenDrafts.Modules.Drafts.Features.Drafts.SetDraftPartStatus;

internal sealed record SetDraftPartStatusRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; init; } = default!;

  [FromRoute(Name = "partIndex")]
  public int PartIndex { get; init; } = default!;

  public DraftPartStatusAction Action { get; init; }
}
