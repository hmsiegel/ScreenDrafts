namespace ScreenDrafts.Modules.RealTimeUpdates.Features.DraftParts;

internal static class GamePlayTokenQuery
{
  public const string Sql = """
    SELECT
      dpp.participant_id_value    AS ParticipantIdValue,
      dpp.participant_kind_value  AS ParticipantKindValue,
      (dpp.starting_vetoes + dpp.vetoes_rolling_in + dpp.awarded_vetoes - dpp.vetoes_used)
                                  AS VetoTokensRemaining,
      (dpp.veto_overrides_rolling_in + dpp.awarded_veto_overrides - dpp.veto_overrides_used)
                                  AS OverrideTokensRemaining
    FROM drafts.draft_part_participants dpp
    JOIN drafts.draft_parts dp ON dp.id = dpp.draft_part_id
    WHERE dp.public_id = @DraftPartPublicId
      AND dpp.participant_kind_value != 2
    """;

  public sealed record TokenRow(
    Guid ParticipantIdValue,
    int ParticipantKindValue,
    int VetoTokensRemaining,
    int OverrideTokensRemaining
  );
}
