namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.CreateDraft;

public sealed record CreateDraftCommand(
  string Title,
  DraftType DraftType,
  Guid SeriesId,
  int TotalPicks,
  int TotalDrafters,
  int TotalDrafterTeams,
  int TotalHosts,
  bool AutoCreateFirstPart = true) : ICommand<Guid>;
