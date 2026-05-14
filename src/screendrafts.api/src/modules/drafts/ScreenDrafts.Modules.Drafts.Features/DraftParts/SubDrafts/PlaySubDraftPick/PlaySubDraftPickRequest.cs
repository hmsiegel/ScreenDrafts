namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SubDrafts.PlaySubDraftPick;

internal sealed record PlaySubDraftPickRequest
{
  [FromRoute(Name = "draftPartId")]
  public string DraftPartPublicId { get; init; } = default!;

  [FromRoute(Name = "subDraftId")]
  public string SubDraftPublicId { get; init; } = default!;

  public required string MoviePublicId { get; init; }
  public required int Position { get; init; }
  public required int PlayOrder { get; init; }
  public required string ParticipantPublicId { get; init; }
  public required int ParticipantKind { get; init; }
}
