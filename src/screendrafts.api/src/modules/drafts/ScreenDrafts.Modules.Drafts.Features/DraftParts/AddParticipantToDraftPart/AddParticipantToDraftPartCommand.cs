namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.AddParticipantToDraftPart;

internal sealed record AddParticipantToDraftPartCommand : ICommand
{
  public Guid DraftPartId { get; init; }
  public string? ParticipantPublicId { get; init; }
  public ParticipantKind ParticipantKind { get; init; } = default!;
}

