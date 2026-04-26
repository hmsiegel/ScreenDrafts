namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks.RevealPick;

internal sealed record RevealPickRequest
{
  [FromRoute(Name = "draftPartId")]
  public required string DraftPartId { get; init; }

  [FromRoute(Name = "playOrder")]
  public required int PlayOrder { get; init; }
}
