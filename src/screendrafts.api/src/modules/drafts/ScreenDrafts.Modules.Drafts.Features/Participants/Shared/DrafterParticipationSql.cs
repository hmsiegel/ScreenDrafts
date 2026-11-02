// Participants/Shared/DrafterParticipationSql.cs
namespace ScreenDrafts.Modules.Drafts.Features.Participants.Shared;

/// <summary>
/// Shared SQL for per-drafter stats. A drafter takes part in a draft part either solo
/// (a kind-0 draft_part_participants row) or through a team (a kind-1 row). Team
/// participation counts toward the individual: appearance, picks, vetoes, veto overrides
/// and commissioner overrides all credit every member.
///
/// Membership is derived from team_pick_credits — the snapshot taken when each team pick
/// was played — never from the team's current roster, which drifts over time. A team
/// counts for a drafter in a part when at least one of that team's picks in the part
/// credits them.
///
/// Vetoes used does not count a veto that was overridden. It is the larger of the
/// vetoes that stood (rows not overridden) and the counter minus the overridden rows.
/// The counter alone undercounts historical rows where the veto was recorded without
/// spending the counter, and it counts overridden vetoes (they were spent). The first
/// term fixes the undercount, the subtraction removes the overridden ones, and a counter
/// above the row count (a veto spent with no row recorded) still wins over the rows.
/// Veto overrides used keep the larger of the counter and the override rows.
///
/// Rollover is deliberately NOT derived from these rows: vetoes and overrides awarded in
/// team drafts typically do not roll over, so rollover queries keep reading kind-0 rows only.
/// </summary>
internal static class DrafterParticipationSql
{
  /// <summary>
  /// Derived table, one row per draft part the drafter took part in, limited to
  /// @AllowedChannels. Correlated: reads the drafter's id from the outer alias <c>d</c>
  /// (<c>d.id</c>). Columns: dpp_id, draft_part_id, draft_id, is_team, vetoes_used,
  /// veto_overrides_used, commissioner_overrides.
  /// </summary>
  public const string Slots = """
    SELECT
      dpp.id                      AS dpp_id,
      dpp.draft_part_id           AS draft_part_id,
      dp.draft_id                 AS draft_id,
      false                       AS is_team,
      GREATEST(
        (SELECT COUNT(*)::int FROM drafts.vetoes v
         WHERE v.issued_by_participant_id = dpp.id AND NOT v.is_overridden),
        dpp.vetoes_used
          - (SELECT COUNT(*)::int FROM drafts.vetoes v
             WHERE v.issued_by_participant_id = dpp.id AND v.is_overridden)) AS vetoes_used,
      GREATEST(dpp.veto_overrides_used,
        (SELECT COUNT(*)::int FROM drafts.veto_overrides vo
         WHERE vo.issued_by_participant_id = dpp.id))          AS veto_overrides_used,
      dpp.commissioner_overrides  AS commissioner_overrides
    FROM drafts.draft_part_participants dpp
    JOIN drafts.draft_parts dp ON dp.id = dpp.draft_part_id
    WHERE dpp.participant_kind_value = 0
      AND dpp.participant_id_value = d.id
      AND EXISTS (
        SELECT 1 FROM drafts.draft_releases dr
        WHERE dr.part_id = dpp.draft_part_id
          AND dr.release_channel = ANY(@AllowedChannels)
      )
    UNION ALL
    SELECT
      dpp.id,
      dpp.draft_part_id,
      dp.draft_id,
      true,
      GREATEST(
        (SELECT COUNT(*)::int FROM drafts.vetoes v
         WHERE v.issued_by_participant_id = dpp.id AND NOT v.is_overridden),
        dpp.vetoes_used
          - (SELECT COUNT(*)::int FROM drafts.vetoes v
             WHERE v.issued_by_participant_id = dpp.id AND v.is_overridden)),
      GREATEST(dpp.veto_overrides_used,
        (SELECT COUNT(*)::int FROM drafts.veto_overrides vo
         WHERE vo.issued_by_participant_id = dpp.id)),
      dpp.commissioner_overrides
    FROM drafts.draft_part_participants dpp
    JOIN drafts.draft_parts dp ON dp.id = dpp.draft_part_id
    WHERE dpp.participant_kind_value = 1
      AND EXISTS (
        SELECT 1
        FROM drafts.picks tp
        JOIN drafts.team_pick_credits tpc ON tpc.target_pick_id = tp.id
        WHERE tp.played_by_participant_id = dpp.id
          AND tpc.drafter_id_value = d.id
      )
      AND EXISTS (
        SELECT 1 FROM drafts.draft_releases dr
        WHERE dr.part_id = dpp.draft_part_id
          AND dr.release_channel = ANY(@AllowedChannels)
      )
    """;

  /// <summary>
  /// Statement prefix for queries that take @DrafterId: defines the CTE
  /// <c>drafter_slots</c> with the columns of <see cref="Slots"/>.
  /// </summary>
  public const string WithSlots = $"""
    WITH drafter_slots AS (
      SELECT s.*
      FROM (SELECT @DrafterId::uuid AS id) d,
      LATERAL ({Slots}) s
    )
    """;

  /// <summary>
  /// Predicate on picks aliased <c>pk</c>: the pick was played by the drafter, or by a
  /// team the drafter was credited on in that part. Needs the CTE <c>drafter_slots</c>.
  /// Already confined to the allowed channels.
  /// </summary>
  public const string OwnPicks = """
    pk.played_by_participant_id IN (SELECT dpp_id FROM drafter_slots)
    """;
}
