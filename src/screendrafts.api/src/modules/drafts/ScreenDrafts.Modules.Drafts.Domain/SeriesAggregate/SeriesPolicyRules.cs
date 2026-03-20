namespace ScreenDrafts.Modules.Drafts.Domain.SeriesAggregate;

public static class SeriesPolicyRules
{
  // Part Budget

  /// <summary>
  /// Computes the per-part veto/override budget from draft type and total participants.
  /// Community picks/vetoes are administator-controlled on the DraftPart directly
  /// and are not part of this computation.
  /// </summary>
  /// <param name="draftType"></param>
  /// <param name="totalParticipants"></param>
  /// <returns></returns>
  public static PartBudget ComputePartBudget(DraftType draftType, int totalParticipants)
  {
    ArgumentNullException.ThrowIfNull(draftType);

    return new PartBudget(
      MaxVetoes: ComputeMaxVetoes(draftType, totalParticipants),
      MaxVetoOverrides: ComputeMaxVetoOverrides(draftType, totalParticipants));
  }

  // Canonical Eligibility
  public static bool IsDraftPartCanon(CanonicalPolicy policy, IReadOnlyCollection<DraftRelease> releases)
  {
    ArgumentNullException.ThrowIfNull(policy);
    ArgumentNullException.ThrowIfNull(releases);

    return policy.Value switch
    {
      // Always: every completed draft in this series is canon
      0 => true,

      // Never: no draft in this series is canon; Legends, Franchise mini-Super, Best Picture Nominee
      1 => false,

      // OnMainFeed: Patreon drafts that become canon when they appear on the main feed.
      2 => releases.Any(r => r.ReleaseChannel == ReleaseChannel.MainFeed),

      _ => false
    };
  }

  // Private Helpers
  private static int ComputeMaxVetoes(DraftType draftType, int totalParticipants)
  {
    // Standard: starting veto (1) + max rollover (1) per participant = 2 per participant
    // Mega/ Super/ mini-Mega: base + 1 awarded across the part
    // mini-Super: fixed 2 (2-participant draft, no rollovers)
    // Speed Draft: 1 per participant; intra-draft rollover only, no carryover across parts (not yet modeled)
    return draftType.Value switch
    {
      0 => 2 * totalParticipants, // Standard
      1 or 2 or 3 => (totalParticipants * 2) + 1, // Mega, Super, mini-Mega
      4 => totalParticipants, // mini-Super
      5 => totalParticipants, // Speed Draft (not yet modeled)
      _ => totalParticipants * 2
    };
  }

  private static int ComputeMaxVetoOverrides(DraftType draftType, int totalParticipants)
  {
    // Standard / mini-Super / Speed Draft: no veto overrides
    // Mega/ Super/ mini-Mega: participants + 1 awarded across the part
    // Exception: 2-participant drafts get no overrides
    return draftType.Value switch
    {
      1 or 2 or 3 => totalParticipants <= 2 ? 0 : totalParticipants + 1, // Mega, Super, mini-Mega
      _ => 0
    };
  }
}
