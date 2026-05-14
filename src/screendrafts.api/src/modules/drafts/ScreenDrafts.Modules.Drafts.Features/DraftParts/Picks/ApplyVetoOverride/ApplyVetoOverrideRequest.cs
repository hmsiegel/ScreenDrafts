namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks.ApplyVetoOverride;

internal sealed record ApplyVetoOverrideRequest
{
  [FromRoute(Name = "draftPartId")]
  public string DraftPartId { get; init; } = default!;

  [FromRoute(Name = "playOrder")]
  public int PlayOrder { get; init; } = default!;
  public string? ParticipantIdValue { get; init; }
  public required int ParticipantKind { get; init; }
}
