namespace ScreenDrafts.Modules.Drafts.Features.Drafts.ListUpcomingDrafts;

internal sealed record DraftUserCapabilities(
  string? Role,
  bool CanEdit,
  bool CanDelete,
  bool CanStart,
  bool CanUpdateBoard,
  bool CanJoin);
