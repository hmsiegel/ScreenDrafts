namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SubDrafts.PlaySubDraftPick;

internal sealed record PlaySubDraftPickRequest
{
  [FromRoute(Name = "draftPartId")]
  public required string DraftPartPublicId { get; init; }

  [FromRoute(Name = "subDraftId")]
  public required string SubDraftPublicId { get; init; }

  public required string MoviePublicId { get; init; }
  public required int Position { get; init; }
  public required int PlayOrder { get; init; }
  public required string ParticipantPublicId { get; init; }
  public required int ParticipantKind { get; init; }
}
