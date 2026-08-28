namespace ScreenDrafts.Modules.Drafts.Features.Predictions;

/// <summary>
/// Config-driven, permanent scoring exceptions for the predictions game.
/// Bound from "Shared:Predictions" — same convention as DraftsOptions's
/// "Shared:People" (CommissionerPersonPublicIds), sourced via Vault in every
/// environment except Testing (see Program.cs / vault-init.sh, key
/// "secret/screendrafts/drafts").
///
/// ShootTheMoonIneligibleContestantIds holds raw PredictionContestant primary
/// key GUIDs (not public IDs) so the scoring handler can match against
/// DraftPredictionSet.ContestantId — a scalar property that's always
/// populated regardless of which navigations a repository's Include chain
/// loaded, unlike Contestant.Person.PublicId.
/// </summary>
internal sealed class PredictionsOptions
{
  public const string SectionName = "Shared:Predictions";

  /// <summary>
  /// Contestants permanently excluded from the Shoot the Moon bonus,
  /// regardless of how many predictions they get right. Currently just
  /// Clay Keller — a standing house rule, not a per-season/per-draft toggle.
  /// </summary>
  public Guid[] ShootTheMoonIneligibleContestantIds { get; set; } = [];
}
