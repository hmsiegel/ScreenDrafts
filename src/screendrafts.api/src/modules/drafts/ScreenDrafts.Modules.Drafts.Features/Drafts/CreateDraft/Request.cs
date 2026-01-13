namespace ScreenDrafts.Modules.Drafts.Features.Drafts.CreateDraft;

public sealed record Request(
  string Title,
  int DraftType,
  Guid SeriesId,
  int TotalPicks,
  int TotalDrafters,
  int TotalDrafterTeams,
  int TotalHosts,
  bool AutoCreateFirstPart = true
);
