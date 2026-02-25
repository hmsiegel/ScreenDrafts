namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.ApplyVetoOverride;

internal sealed record ApplyVetoOverrideCommand : ICommand
{
  public required string DraftPartId { get; init; }
  public required int PlayOrder { get; init; }
  public string? ParticipantIdValue { get; init; }
  public required ParticipantKind ParticipantKind { get; init; }
}
