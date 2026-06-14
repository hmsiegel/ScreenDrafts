namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Participants.RemoveParticipantFromDraftPart;

internal sealed record RemoveParticipantFromDraftPartCommand : ICommand
{
  public required string DraftPartId { get; init; }
  public string? ParticipantPublicId { get; init; }
  public required ParticipantKind ParticipantKind { get; init; }
}
