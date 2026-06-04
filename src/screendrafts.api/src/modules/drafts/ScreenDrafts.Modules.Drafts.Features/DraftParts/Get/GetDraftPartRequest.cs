namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Get;

internal sealed record GetDraftPartRequest
{
  [FromRoute(Name = "draftPartId")]
  public string DraftPartId { get; init; } = default!;
}
