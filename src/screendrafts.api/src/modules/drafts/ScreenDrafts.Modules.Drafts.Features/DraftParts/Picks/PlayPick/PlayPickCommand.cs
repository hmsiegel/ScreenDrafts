namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks.PlayPick;

internal sealed record PlayPickCommand : ICommand<PickId>
{
  public required string DraftPartId { get; init; }
  public required int Position { get; init; }
  public required int PlayOrder { get; init; }
  public string? ParticipantPublicId { get; init; } = default!;
  public ParticipantKind ParticipantKind { get; init; } = default!;
  public Guid MovieId { get; init; }
  public string? MovieVersionName { get; init; } = default!;
  public string ActedByPublicId { get; init; } = default!;
}
