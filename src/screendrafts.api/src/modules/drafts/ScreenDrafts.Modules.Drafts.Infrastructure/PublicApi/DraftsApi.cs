namespace ScreenDrafts.Modules.Drafts.Infrastructure.PublicApi;

internal sealed class DraftsApi(IDbConnectionFactory connectionFactory) : IDraftsApi
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

  public async Task<IReadOnlyList<MediaAppearanceRecord>> GetMediaAppearancesAsync(
    string mediaPublicId,
    bool includePatreon,
    CancellationToken ct = default
  )
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(ct);

    // One row per pick. A pick may have zero or one veto; a veto may have
    // zero or one veto_override. commissioner_overrides carry no issuer.
    //
    // Patreon gate: exclude picks whose draft has ONLY a Patreon
    // channel release and @IncludePatreon = false.
    // A draft with no channel releases at all passes through.
    const string sql = """
      SELECT
          d.public_id                                         AS DraftPublicId,
          d.title                                             AS DraftTitle,
          dcr_ep.episode_number                               AS EpisodeNumber,
          pk.position                                         AS Position,

          -- Picked by: kind 0=Drafter, 1=DrafterTeam, 2=Community
          CASE
              WHEN pk.played_by_participant_kind_value = 2 THEN 'Patreon Members'
              WHEN pk.played_by_participant_kind_value = 1 THEN dt_pick.name
              ELSE COALESCE(
                  per_pick.first_name || ' ' || per_pick.last_name,
                  per_pick.first_name
              )
          END                                                 AS PickedByDisplayName,
          CASE
              WHEN pk.played_by_participant_kind_value = 1 THEN NULL
              ELSE per_pick.public_id
          END                                                 AS PickedByPersonPublicId,

          -- Veto / override flags
          (v.id IS NOT NULL)                                  AS WasVetoed,
          (vo.id IS NOT NULL)                                 AS WasVetoOverridden,
          (co.id IS NOT NULL)                                 AS WasCommissionerOverride,

          -- Vetoed by (kind 0=Drafter, 1=DrafterTeam, 2=Community)
          CASE
              WHEN v.id IS NULL THEN NULL
              WHEN dpp_veto.participant_kind_value = 2 THEN 'Patreon Members'
              WHEN dpp_veto.participant_kind_value = 1 THEN dt_veto.name
              ELSE COALESCE(
                  per_veto.first_name || ' ' || per_veto.last_name,
                  per_veto.first_name
              )
          END                                                 AS VetoedByDisplayName,

          -- Veto override by (kind 0=Drafter, 1=DrafterTeam, 2=Community)
          CASE
              WHEN vo.id IS NULL THEN NULL
              WHEN dpp_vo.participant_kind_value = 2 THEN 'Patreon Members'
              WHEN dpp_vo.participant_kind_value = 1 THEN dt_vo.name
              ELSE COALESCE(
                  per_vo.first_name || ' ' || per_vo.last_name,
                  per_vo.first_name
              )
          END                                                 AS VetoOverrideByDisplayName,

          -- Patreon flag: true only if this draft part has a Patreon release
          -- but NO main feed release (i.e. it is exclusively Patreon)
          (
              EXISTS (
                  SELECT 1 FROM drafts.draft_releases dr_p
                  WHERE dr_p.part_id = dp.id AND dr_p.release_channel = 1
              )
              AND NOT EXISTS (
                  SELECT 1 FROM drafts.draft_releases dr_m
                  WHERE dr_m.part_id = dp.id AND dr_m.release_channel = 0
              )
          )                                                   AS IsPatreon

      FROM drafts.picks pk

      -- Movie lookup via internal movie_id
      JOIN drafts.movies mv
          ON mv.id = pk.movie_id
         AND mv.public_id = @MediaPublicId

      -- Draft part + draft
      JOIN drafts.draft_parts dp
          ON dp.id = pk.draft_part_id
      JOIN drafts.drafts d
          ON d.id = dp.draft_id

      -- Episode number from main-feed channel release
      LEFT JOIN drafts.draft_channel_releases dcr_ep
          ON dcr_ep.draft_id = d.id
         AND dcr_ep.release_channel = 0

      -- Picked-by: composite columns on picks → drafters → people (kind=0)
      LEFT JOIN drafts.drafters dr_pick
          ON dr_pick.id = pk.played_by_participant_id_value
         AND pk.played_by_participant_kind_value = 0
      LEFT JOIN drafts.people per_pick
          ON per_pick.id = dr_pick.person_id
      -- Picked-by: drafter team (kind=1)
      LEFT JOIN drafts.drafter_teams dt_pick
          ON dt_pick.id = pk.played_by_participant_id_value
         AND pk.played_by_participant_kind_value = 1

      -- Veto
      LEFT JOIN drafts.vetoes v
          ON v.target_pick_id = pk.id

      -- Veto issuer → draft_part_participants → drafters/teams/community → person
      LEFT JOIN drafts.draft_part_participants dpp_veto
          ON dpp_veto.id = v.issued_by_participant_id
      LEFT JOIN drafts.drafters dr_veto
          ON dr_veto.id = dpp_veto.participant_id_value
         AND dpp_veto.participant_kind_value = 0
      LEFT JOIN drafts.people per_veto
          ON per_veto.id = dr_veto.person_id
      LEFT JOIN drafts.drafter_teams dt_veto
          ON dt_veto.id = dpp_veto.participant_id_value
         AND dpp_veto.participant_kind_value = 1

      -- Veto override
      LEFT JOIN drafts.veto_overrides vo
          ON vo.veto_id = v.id

      -- Veto override issuer → draft_part_participants → drafters/teams/community → person
      LEFT JOIN drafts.draft_part_participants dpp_vo
          ON dpp_vo.id = vo.issued_by_participant_id
      LEFT JOIN drafts.drafters dr_vo
          ON dr_vo.id = dpp_vo.participant_id_value
         AND dpp_vo.participant_kind_value = 0
      LEFT JOIN drafts.people per_vo
          ON per_vo.id = dr_vo.person_id
      LEFT JOIN drafts.drafter_teams dt_vo
          ON dt_vo.id = dpp_vo.participant_id_value
         AND dpp_vo.participant_kind_value = 1

      -- Commissioner override (flag only, no issuer)
      LEFT JOIN drafts.commissioner_overrides co
          ON co.pick_id = pk.id

      -- Patreon gate: show pick if either:
      --   a) user is a Patreon member (see all), or
      --   b) this draft part has a main feed release (canonical), or
      --   c) this draft part has no Patreon release at all
      WHERE (
          @IncludePatreon = true
          OR EXISTS (
              SELECT 1 FROM drafts.draft_releases dr_gate_m
              WHERE dr_gate_m.part_id = dp.id AND dr_gate_m.release_channel = 0
          )
          OR NOT EXISTS (
              SELECT 1 FROM drafts.draft_releases dr_gate_p
              WHERE dr_gate_p.part_id = dp.id AND dr_gate_p.release_channel = 1
          )
      )

      ORDER BY dcr_ep.episode_number DESC NULLS LAST, pk.position ASC NULLS LAST
      """;

    var rows = await connection.QueryAsync<MediaAppearanceRecord>(
      sql,
      new { MediaPublicId = mediaPublicId, IncludePatreon = includePatreon }
    );

    return [.. rows];
  }
}
