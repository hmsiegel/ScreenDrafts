namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SubDrafts.SetSubDraftSubject;

internal sealed record SetSubDraftSubjectRequest
{
  [FromRoute(Name = "draftPartId")]
  public string DraftPartPublicId { get; init; } = default!;

  [FromRoute(Name = "subDraftId")]
  public string SubDraftPublicId { get; init; } = default!;

  public int SubjectKind { get; init; }
  public string SubjectName { get; init; } = default!;
  public string? SubjectImdbId { get; init; }
}
