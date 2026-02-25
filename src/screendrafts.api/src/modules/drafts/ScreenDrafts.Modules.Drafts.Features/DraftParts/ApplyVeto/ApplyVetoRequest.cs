namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.ApplyVeto;

internal sealed record ApplyVetoRequest
{
  [FromRoute(Name = "draftPartId")]
  public required string DraftPartId { get; init; }

  [FromRoute(Name = "playOrder")]
  public int PlayOrder { get; init; }

  public string? ParticipantPublicId { get; init; }
  public int ParticipantKind { get; init; }
}
