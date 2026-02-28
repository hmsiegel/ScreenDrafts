namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.RemoveParticipantFromDraftPart;

internal sealed record RemoveParticipantFromDraftPartCommand : ICommand
{
  public required string DraftPartPublicId { get; init; }
  public string? ParticipantPublicId { get; init; }
  public required ParticipantKind ParticipantKind { get; init; }
}
