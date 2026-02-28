namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.RemoveParticipantFromDraftPart;

internal sealed record RemoveParticipantFromDraftPartRequest
{
  public required string DraftPartPublicId { get; init; }
  public string? ParticipantPublicId { get; init; }
  public required int ParticipantKind { get; init; }
}
