namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks.UndoVeto;

internal sealed record UndoVetoRequest
{
  [FromRoute(Name = "draftPartId")]
  public string DraftPartId { get; set; } = default!;

  [FromRoute(Name = "playOrder")]
  public int PlayOrder { get; set; }
  public string? SubDraftPublicId { get; set; }
}
