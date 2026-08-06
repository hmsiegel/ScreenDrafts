namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.GamePlay.GetDraftPartGamePlay;

internal sealed record GameplaySubDraftSummaryResponse
{
  public string PublicId { get; init; } = default!;
  public int Index { get; init; }
  public int Status { get; init; }
  public int? SubjectKind { get; init; }
  public string? SubjectName { get; init; }
  public string? SubjectImdbId { get; init; }
}
