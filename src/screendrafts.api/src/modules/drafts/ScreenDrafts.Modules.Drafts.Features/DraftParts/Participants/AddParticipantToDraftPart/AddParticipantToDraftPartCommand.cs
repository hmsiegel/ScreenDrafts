namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Participants.AddParticipantToDraftPart;

internal sealed record AddParticipantToDraftPartCommand : ICommand
{
  public required string DraftPartId { get; init; }
  public string? ParticipantPublicId { get; init; }
  public ParticipantKind ParticipantKind { get; init; } = default!;
}

