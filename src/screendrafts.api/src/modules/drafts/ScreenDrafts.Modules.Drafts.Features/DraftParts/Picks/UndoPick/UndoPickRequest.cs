namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks.UndoPick;

internal sealed record UndoPickRequest
{
  [FromRoute(Name = "draftPartId")]
  public required string DraftPartPublicId { get; init; }

  [FromRoute(Name = "playOrder")]
  public int PlayOrder { get; init; }

  /// <summary>
  /// Only required for speed draft picks
  /// </summary>
  public string? SubDraftPublicId { get; init; }
}
