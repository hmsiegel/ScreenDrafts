namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SubDrafts.SetSubDraftSubject;

internal sealed record SetSubDraftSubjectRequest
{
  [FromRoute(Name = "draftPartId")]
  public required string DraftPartPublicId { get; init; }

  [FromRoute(Name = "subDraftId")]
  public required string SubDraftPublicId { get; init; }

  public required int SubjectKind { get; init; }
  public required string SubjectName { get; init; }
}
