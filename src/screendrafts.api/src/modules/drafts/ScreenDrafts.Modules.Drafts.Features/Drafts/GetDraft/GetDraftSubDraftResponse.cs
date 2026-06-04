namespace ScreenDrafts.Modules.Drafts.Features.Drafts.GetDraft;

internal sealed record GetDraftSubDraftResponse
{
  public int Index { get; init; }
  public int SubjectKind { get; init; }
  public string SubjectName { get; init; } = default!;
}
