namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.AddDraftPositionsToGameBoard;

public sealed record DraftPositionRequst(
  string name,
  IEnumerable<int> picks,
  bool hasBonusVeto = false,
  bool hasBonusVetoOverride = false);
