namespace ScreenDrafts.Modules.Drafts.IntegrationEvents;

/// <summary>
/// One veto on a pick. IdValue is drafts.drafters.id when the kind is 0 (drafter) and
/// drafts.drafter_teams.id when the kind is 1 (team). Kind 2 is community.
/// The OverriddenBy* fields describe who played the veto override, when one exists.
/// </summary>
public sealed record StatsVetoRecord(
  Guid VetoId,
  int Sequence,
  int IssuedByKind,
  Guid IssuedByIdValue,
  string? IssuedByPublicId,
  string IssuedByName,
  bool IsOverridden,
  int? OverriddenByKind,
  Guid? OverriddenByIdValue,
  string? OverriddenByPublicId,
  string? OverriddenByName
);
