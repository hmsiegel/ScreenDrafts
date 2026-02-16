namespace ScreenDrafts.Modules.Drafts.Features.Drafts.CreateDraft;

public sealed record CreateDraftRequest(
  string Title,
  int DraftType,
  Guid SeriesId,
  int MinPosition,
  int MaxPosition,
  bool AutoCreateFirstPart = true
);

