namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks.ApplyVeto;

internal sealed record ApplyVetoCommand : ICommand
{
  public required string DraftPartId { get; init; }
  public required int PlayOrder { get; init; }
  public  string? ParticipantPublicId { get; init; }
  public required ParticipantKind ParticipantKind { get; init; } = default!;
  public required string ActorPublicId { get; init; }
}
