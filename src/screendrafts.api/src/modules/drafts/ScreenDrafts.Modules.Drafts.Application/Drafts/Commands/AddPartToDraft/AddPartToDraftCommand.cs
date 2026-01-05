namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.AddPartToDraft;

public sealed record AddPartToDraftCommand(
  Guid DraftId,
  int PartIndex,
  int TotalPicks,
  int TotalDrafters,
  int TotalDrafterTeams,
  int TotalHosts) : ICommand;
