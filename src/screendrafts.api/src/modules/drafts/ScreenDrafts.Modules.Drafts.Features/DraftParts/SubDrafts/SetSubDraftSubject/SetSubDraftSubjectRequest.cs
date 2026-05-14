namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SubDrafts.SetSubDraftSubject;

internal sealed record SetSubDraftSubjectRequest
{
  [FromRoute(Name = "draftPartId")]
  public string DraftPartPublicId { get; init; } = default!;

  [FromRoute(Name = "subDraftId")]
  public string SubDraftPublicId { get; init; } = default!;

  public required int SubjectKind { get; init; }
  public required string SubjectName { get; init; }
}
