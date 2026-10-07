namespace ScreenDrafts.Modules.Drafts.IntegrationEvents;

/// <summary>
/// One drafter credited for a pick. Solo picks credit the player; team picks credit each
/// member snapshotted in team_pick_credits. Community picks credit no one.
/// </summary>
public sealed record StatsCreditRecord(
  Guid DrafterIdValue,
  string DrafterPublicId,
  string PersonPublicId,
  string DrafterName
);
