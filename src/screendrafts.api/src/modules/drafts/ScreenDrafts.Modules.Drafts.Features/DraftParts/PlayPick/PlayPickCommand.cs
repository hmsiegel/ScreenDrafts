namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.PlayPick;

internal sealed record PlayPickCommand : ICommand<PickId>
{
  public required Guid DraftPartId { get; init;  }
  public required int Position { get; init; }
  public required int PlayOrder { get; init; }
  public string? ParticipantPublicId { get; init; } = default!;
  public ParticipantKind ParticipantKind { get; init; } = default!;
  public Guid MovieId { get; init; }
  public string? MovieVersionName { get; init; } = default!;
}
