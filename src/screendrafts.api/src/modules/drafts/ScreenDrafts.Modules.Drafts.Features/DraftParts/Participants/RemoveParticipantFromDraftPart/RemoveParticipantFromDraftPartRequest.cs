namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Participants.RemoveParticipantFromDraftPart;

internal sealed record RemoveParticipantFromDraftPartRequest
{
  [FromRoute(Name = "draftPartId")]
  public string DraftPartId { get; init; } = default!;
  public string? ParticipantPublicId { get; init; }
  public required int ParticipantKind { get; init; }
}
