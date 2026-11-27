namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Participants.SetPhantomParticipant;

internal sealed record SetPhantomParticipantCommand : ICommand
{
  public required string DraftPartId { get; init; }
  public required string ParticipantPublicId { get; init; }
  public required bool IsPhantom { get; init; }
}
