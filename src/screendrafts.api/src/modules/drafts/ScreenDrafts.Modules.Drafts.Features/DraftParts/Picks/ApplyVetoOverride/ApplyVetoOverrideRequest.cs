namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks.ApplyVetoOverride;

internal sealed record ApplyVetoOverrideRequest
{
  [FromRoute(Name = "draftPartId")]
  public required string DraftPartId { get; init; }

  [FromRoute(Name = "playOrder")]
  public required int PlayOrder { get; init; }
  public string? ParticipantIdValue { get; init; }
  public required int ParticipantKind { get; init; }
}
