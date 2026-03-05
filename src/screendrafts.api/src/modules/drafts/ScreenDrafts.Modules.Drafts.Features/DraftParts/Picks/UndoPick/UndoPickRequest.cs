namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks.UndoPick;

internal sealed record UndoPickRequest
{
  [FromRoute(Name = "draftPartId")]
  public required string DraftPartPublicId { get; init; }

  [FromRoute(Name = "playOrder")]
  public int PlayOrder { get; init; }
}
