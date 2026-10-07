namespace ScreenDrafts.Modules.Drafts.IntegrationEvents;

/// <summary>
/// One pick played in a draft part, including picks that were vetoed or removed by
/// commissioner override. Vetoes are ordered by Sequence.
/// </summary>
public sealed record StatsPickRecord(
  Guid PickId,
  int Position,
  int PlayOrder,
  int? SubDraftIndex,
  string MediaPublicId,
  string MediaTitle,
  int PlayedByKind,
  Guid PlayedByIdValue,
  string? PlayedByPublicId,
  string PlayedByName,
  bool IsCommissionerOverridden,
  IReadOnlyList<StatsVetoRecord> Vetoes,
  IReadOnlyList<StatsCreditRecord> Credits
);
