namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.EditDraft;

public sealed record EditDraftCommand(
  Guid DraftId,
  string Title,
  DraftType DraftType,
  int TotalPicks,
  int TotalDrafters,
  int TotalDrafterTeams,
  int TotalHosts,
  EpisodeType EpisodeType,
  DraftStatus DraftStatus,
  string? Description) : ICommand;
