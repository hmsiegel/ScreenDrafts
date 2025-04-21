﻿namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.AddDraftPositionsToGameBoard;

public sealed record DraftPositionRequest(
  string name,
  IEnumerable<int> picks,
  bool hasBonusVeto = false,
  bool hasBonusVetoOverride = false);
