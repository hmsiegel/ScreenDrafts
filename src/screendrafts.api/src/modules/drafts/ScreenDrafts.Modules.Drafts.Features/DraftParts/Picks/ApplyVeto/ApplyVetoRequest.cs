namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks.ApplyVeto;

internal sealed record ApplyVetoRequest
{
  [FromRoute(Name = "draftPartId")]
  public string DraftPartId { get; init; } = default!;

  [FromRoute(Name = "playOrder")]
  public int PlayOrder { get; init; }

  public string? ParticipantPublicId { get; init; }
  public int ParticipantKind { get; init; }
}
