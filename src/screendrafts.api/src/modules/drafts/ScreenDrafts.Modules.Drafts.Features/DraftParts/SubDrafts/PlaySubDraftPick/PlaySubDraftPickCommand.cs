namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SubDrafts.PlaySubDraftPick;

internal sealed record PlaySubDraftPickCommand : ICommand<PickId>
{

  public required string DraftPartPublicId { get; init; }
  public required string SubDraftPublicId { get; init; }
  public required string MoviePublicId { get; init; }
  public required int Position { get; init; }
  public required int PlayOrder { get; init; }
  public required string ParticipantPublicId { get; init; }
  public required ParticipantKind ParticipantKind { get; init; }
  public string? ActedByPublicId { get; init; }
}
