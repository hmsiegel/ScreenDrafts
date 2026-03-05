namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks.ApplyCommissionerOverride;

internal sealed record ApplyCommissionerOverrideRequest
{
  [FromRoute(Name = "draftPartId")]
  public required string DraftPartId { get; init; }

  [FromRoute(Name = "playOrder")]
  public required int PlayOrder { get; init; }
}


