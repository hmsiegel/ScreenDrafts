namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.CreateGameBoard;

public sealed record DraftPositionDto(
  string Name,
  IEnumerable<int> Picks,
  bool HasBonusVeto,
  bool HasBonusVetoOverride);
