namespace ScreenDrafts.Modules.Drafts.IntegrationEvents;

public sealed record CompletedSubDraftBreakdownRecord(
  string SubDraftPublicId,
  int Index,
  int? SubjectKind,
  string? SubjectName,
  IReadOnlyList<CompletedPickRecord> Picks
);
