namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.CreateDraft;

public sealed record CreateDraftCommand(
  string Title,
  DraftType DraftType,
  int TotalPicks,
  int TotalDrafters,
  int TotalHosts,
  EpisodeType EpisodeType,
  DraftStatus DraftStatus) : ICommand<Guid>;
