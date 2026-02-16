namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Queries.ListUpcomingDrafts;

public sealed record DraftUserCapabilities(
  string? Role,
  bool CanEdit,
  bool CanDelete,
  bool CanStart,
  bool CanPlay);
