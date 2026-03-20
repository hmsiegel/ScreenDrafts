namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Participants.AddParticipantToDraftPart;

internal sealed record AddParticipantToDraftPartRequest
{
  [FromRoute(Name = "draftPartId")]
  public string DraftPartId { get; init; } = default!;

  public string? ParticipantPublicId { get; init; }
  public int ParticipantKind { get; init; }
}


