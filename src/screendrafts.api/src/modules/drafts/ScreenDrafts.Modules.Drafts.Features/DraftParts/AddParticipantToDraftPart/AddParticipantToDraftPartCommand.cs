namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.AddParticipantToDraftPart;

internal sealed record AddParticipantToDraftPartCommand : ICommand
{
  public required string DraftPartPublicId { get; init; }
  public string? ParticipantPublicId { get; init; }
  public ParticipantKind ParticipantKind { get; init; } = default!;
}

