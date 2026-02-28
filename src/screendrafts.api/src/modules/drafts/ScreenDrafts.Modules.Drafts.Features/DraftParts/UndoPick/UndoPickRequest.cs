namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.UndoPick;

internal sealed record UndoPickRequest
{
  [FromRoute(Name = "draftPartId")]
  public required string DraftPartPublicId { get; init; }

  [FromRoute(Name = "playOrder")]
  public int PlayOrder { get; init; }
}
