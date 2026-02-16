namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Commands.CreateGameBoard;

public sealed record DraftPositionDto(
  string Name,
  IEnumerable<int> Picks,
  bool HasBonusVeto,
  bool HasBonusVetoOverride);
