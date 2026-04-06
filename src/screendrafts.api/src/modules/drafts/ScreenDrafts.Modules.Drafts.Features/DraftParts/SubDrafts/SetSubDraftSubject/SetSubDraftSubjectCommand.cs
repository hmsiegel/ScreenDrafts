namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SubDrafts.SetSubDraftSubject;

internal sealed record SetSubDraftSubjectCommand : ICommand
{
  public required string DraftPartPublicId { get; init; }
  public required string SubDraftPublicId { get; init; }
  public required int SubjectKind { get; init; }
  public required string SubjectName { get; init; }
}
