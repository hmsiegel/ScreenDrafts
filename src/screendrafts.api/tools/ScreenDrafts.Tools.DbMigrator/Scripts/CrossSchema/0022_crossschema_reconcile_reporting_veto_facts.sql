-- Reconciles Reporting with the corrected Drafts data (Drafts scripts 0059-0066).
-- Sanctioned cross-schema script (runs as the superuser). Idempotent.
--
-- 1. Finds completed parts (reporting.draft_summaries) whose pick_facts / veto_facts disagree with
--    drafts.picks / drafts.vetoes: missing or extra rows, or a different position, play_order,
--    veto count or override flag. After Drafts 0061 this is TV Adaptations part 1 (Drew
--    McWeeny's self-vetoed Mission: Impossible III, plus the shifted play_order values).
-- 2. Replaces pick_facts, veto_facts and pick_credit_facts for those parts only, exactly as
--    RecordPartStatsCommandHandler does.
-- 3. Sets reporting.site_stats.vetoes_count to the number of non-overridden vetoes across
--    completed parts (the figure UpsertDraftSummary adds up one part at a time).
--
-- Not touched: draft_summaries.total_picks, canonical appearances, honorifics. They exclude
-- vetoed picks, so the new pick changes none of them.
-- After running, clear the Redis cache (site stats 24h key, record-book keys); see the notes.
--
-- Participant kinds: 0 = drafter, 1 = team, 2 = community.
-- No explicit transaction: the DbMigrator cross-schema runner wraps every script in one.

DROP TABLE IF EXISTS pg_temp._stale_parts;

CREATE TEMP TABLE _stale_parts AS
WITH parts AS (
  SELECT dp.id AS part_id, dp.public_id AS part_public_id
  FROM drafts.draft_parts dp
  JOIN drafts.drafts d ON d.id = dp.draft_id AND d.is_deleted = false
  WHERE dp.public_id IN (SELECT draft_part_public_id FROM reporting.draft_summaries)
),
src_picks AS (
  SELECT pa.part_public_id, p.id, p.position, p.play_order,
         (SELECT COUNT(*) FROM drafts.vetoes v WHERE v.target_pick_id = p.id)::int AS veto_count,
         COALESCE((SELECT v.is_overridden FROM drafts.vetoes v
                   WHERE v.target_pick_id = p.id ORDER BY v.sequence DESC LIMIT 1), false) AS overridden
  FROM parts pa
  JOIN drafts.picks p ON p.draft_part_id = pa.part_id
),
src_vetoes AS (
  SELECT sp.part_public_id, v.id, v.is_overridden
  FROM src_picks sp
  JOIN drafts.vetoes v ON v.target_pick_id = sp.id
),
bad AS (
  -- pick in Drafts, absent or different in Reporting
  SELECT sp.part_public_id
  FROM src_picks sp
  LEFT JOIN reporting.pick_facts pf ON pf.id = sp.id
  WHERE pf.id IS NULL
     OR pf.position <> sp.position
     OR pf.play_order <> sp.play_order
     OR pf.veto_count <> sp.veto_count
     OR pf.was_veto_overridden <> sp.overridden
     OR pf.draft_part_public_id <> sp.part_public_id
  UNION
  -- pick in Reporting, gone from Drafts
  SELECT pf.draft_part_public_id
  FROM reporting.pick_facts pf
  WHERE pf.draft_part_public_id IN (SELECT part_public_id FROM parts)
    AND NOT EXISTS (SELECT 1 FROM src_picks sp WHERE sp.id = pf.id)
  UNION
  SELECT sv.part_public_id
  FROM src_vetoes sv
  LEFT JOIN reporting.veto_facts vf ON vf.id = sv.id
  WHERE vf.id IS NULL OR vf.is_overridden <> sv.is_overridden
  UNION
  SELECT vf.draft_part_public_id
  FROM reporting.veto_facts vf
  WHERE vf.draft_part_public_id IN (SELECT part_public_id FROM parts)
    AND NOT EXISTS (SELECT 1 FROM src_vetoes sv WHERE sv.id = vf.id)
)
SELECT DISTINCT part_public_id AS draft_part_public_id FROM bad;

DO $$
DECLARE
  v_n int;
BEGIN
  SELECT COUNT(*) INTO v_n FROM _stale_parts;
  RAISE NOTICE 'Reporting reconcile: % part(s) out of step with Drafts', v_n;
END $$;

DELETE FROM reporting.pick_credit_facts
WHERE draft_part_public_id IN (SELECT draft_part_public_id FROM _stale_parts);

DELETE FROM reporting.veto_facts
WHERE draft_part_public_id IN (SELECT draft_part_public_id FROM _stale_parts);

DELETE FROM reporting.pick_facts
WHERE draft_part_public_id IN (SELECT draft_part_public_id FROM _stale_parts);

-- 1. pick_facts
INSERT INTO reporting.pick_facts
  (id, draft_id, draft_public_id, draft_part_public_id, part_index, draft_title,
   draft_type, series_name, canonical_policy, sub_draft_index, position, play_order,
   media_public_id, media_title, played_by_kind, played_by_id_value,
   played_by_public_id, played_by_name, veto_count, was_vetoed,
   was_veto_overridden, was_commissioner_overridden, recorded_at_utc)
SELECT
  p.id, d.id, d.public_id, dp.public_id, dp.part_index, d.title,
  CASE dp.draft_type
    WHEN 0 THEN 'Standard' WHEN 1 THEN 'MiniMega' WHEN 2 THEN 'Mega'
    WHEN 3 THEN 'Super' WHEN 4 THEN 'MiniSuper' WHEN 5 THEN 'SpeedDraft'
    ELSE dp.draft_type::text
  END,
  s.name, s.canonical_policy, sd.index, p.position, p.play_order,
  m.public_id, m.movie_title,
  p.played_by_participant_kind_value, p.played_by_participant_id_value,
  CASE p.played_by_participant_kind_value WHEN 0 THEN dr.public_id WHEN 1 THEN t.public_id ELSE NULL END,
  CASE p.played_by_participant_kind_value
    WHEN 0 THEN COALESCE(NULLIF(pe.display_name, ''),
                         NULLIF(TRIM(CONCAT_WS(' ', pe.first_name, pe.last_name)), ''),
                         'Unknown drafter')
    WHEN 1 THEN COALESCE(t.name, 'Unknown team')
    ELSE 'Community'
  END,
  (SELECT COUNT(*) FROM drafts.vetoes v WHERE v.target_pick_id = p.id)::int,
  EXISTS (SELECT 1 FROM drafts.vetoes v WHERE v.target_pick_id = p.id),
  COALESCE((SELECT v.is_overridden FROM drafts.vetoes v
            WHERE v.target_pick_id = p.id ORDER BY v.sequence DESC LIMIT 1), false),
  EXISTS (SELECT 1 FROM drafts.commissioner_overrides co WHERE co.pick_id = p.id),
  now()
FROM drafts.picks p
JOIN drafts.draft_parts dp ON dp.id = p.draft_part_id
JOIN drafts.drafts d ON d.id = dp.draft_id AND d.is_deleted = false
JOIN drafts.series s ON s.id = d.series_id
JOIN drafts.movies m ON m.id = p.movie_id
LEFT JOIN drafts.sub_drafts sd ON sd.id = p.sub_draft_id
LEFT JOIN drafts.drafters dr
  ON p.played_by_participant_kind_value = 0 AND dr.id = p.played_by_participant_id_value
LEFT JOIN drafts.people pe ON pe.id = dr.person_id
LEFT JOIN drafts.drafter_teams t
  ON p.played_by_participant_kind_value = 1 AND t.id = p.played_by_participant_id_value
WHERE dp.public_id IN (SELECT draft_part_public_id FROM _stale_parts);

-- 2. veto_facts
INSERT INTO reporting.veto_facts
  (id, pick_id, draft_id, draft_part_public_id, sequence, issued_by_kind,
   issued_by_id_value, issued_by_public_id, issued_by_name, is_overridden,
   overridden_by_kind, overridden_by_id_value, overridden_by_public_id,
   overridden_by_name, is_self_veto, recorded_at_utc)
SELECT
  v.id, p.id, d.id, dp.public_id, v.sequence,
  ip.participant_kind_value, ip.participant_id_value,
  CASE ip.participant_kind_value WHEN 0 THEN idr.public_id WHEN 1 THEN it.public_id ELSE NULL END,
  CASE ip.participant_kind_value
    WHEN 0 THEN COALESCE(NULLIF(ipe.display_name, ''),
                         NULLIF(TRIM(CONCAT_WS(' ', ipe.first_name, ipe.last_name)), ''),
                         'Unknown drafter')
    WHEN 1 THEN COALESCE(it.name, 'Unknown team')
    ELSE 'Community'
  END,
  v.is_overridden,
  op.participant_kind_value, op.participant_id_value,
  CASE op.participant_kind_value WHEN 0 THEN odr.public_id WHEN 1 THEN ot.public_id ELSE NULL END,
  CASE
    WHEN op.id IS NULL THEN NULL
    WHEN op.participant_kind_value = 0 THEN COALESCE(NULLIF(ope.display_name, ''),
                         NULLIF(TRIM(CONCAT_WS(' ', ope.first_name, ope.last_name)), ''),
                         'Unknown drafter')
    WHEN op.participant_kind_value = 1 THEN COALESCE(ot.name, 'Unknown team')
    ELSE 'Community'
  END,
  (ip.participant_id_value = p.played_by_participant_id_value
    AND ip.participant_kind_value = p.played_by_participant_kind_value),
  now()
FROM drafts.vetoes v
JOIN drafts.picks p ON p.id = v.target_pick_id
JOIN drafts.draft_parts dp ON dp.id = p.draft_part_id
JOIN drafts.drafts d ON d.id = dp.draft_id AND d.is_deleted = false
JOIN drafts.draft_part_participants ip ON ip.id = v.issued_by_participant_id
LEFT JOIN drafts.drafters idr ON ip.participant_kind_value = 0 AND idr.id = ip.participant_id_value
LEFT JOIN drafts.people ipe ON ipe.id = idr.person_id
LEFT JOIN drafts.drafter_teams it ON ip.participant_kind_value = 1 AND it.id = ip.participant_id_value
LEFT JOIN drafts.veto_overrides vo ON vo.veto_id = v.id
LEFT JOIN drafts.draft_part_participants op ON op.id = vo.issued_by_participant_id
LEFT JOIN drafts.drafters odr ON op.participant_kind_value = 0 AND odr.id = op.participant_id_value
LEFT JOIN drafts.people ope ON ope.id = odr.person_id
LEFT JOIN drafts.drafter_teams ot ON op.participant_kind_value = 1 AND ot.id = op.participant_id_value
WHERE dp.public_id IN (SELECT draft_part_public_id FROM _stale_parts);

-- 3. pick_credit_facts: solo picks credit the player; team picks credit the snapshotted members.
INSERT INTO reporting.pick_credit_facts
  (id, pick_id, draft_id, draft_part_public_id, drafter_id_value,
   drafter_public_id, drafter_person_public_id, drafter_name, recorded_at_utc)
SELECT
  gen_random_uuid(), c.pick_id, c.draft_id, c.draft_part_public_id, dr.id, dr.public_id,
  COALESCE(pe.public_id, ''),
  COALESCE(NULLIF(pe.display_name, ''),
           NULLIF(TRIM(CONCAT_WS(' ', pe.first_name, pe.last_name)), ''),
           'Unknown drafter'),
  now()
FROM (
  SELECT pf.id AS pick_id, pf.draft_id, pf.draft_part_public_id, pf.played_by_id_value AS drafter_id
  FROM reporting.pick_facts pf
  WHERE pf.played_by_kind = 0
    AND pf.draft_part_public_id IN (SELECT draft_part_public_id FROM _stale_parts)
  UNION
  SELECT pf.id, pf.draft_id, pf.draft_part_public_id, tc.drafter_id_value
  FROM reporting.pick_facts pf
  JOIN drafts.team_pick_credits tc ON tc.target_pick_id = pf.id
  WHERE pf.played_by_kind = 1
    AND pf.draft_part_public_id IN (SELECT draft_part_public_id FROM _stale_parts)
) c
JOIN drafts.drafters dr ON dr.id = c.drafter_id
JOIN drafts.people pe ON pe.id = dr.person_id;

-- 4. site_stats.vetoes_count = non-overridden vetoes across completed parts.
DO $$
DECLARE
  v_expected int;
  v_current int;
BEGIN
  SELECT COUNT(*)::int INTO v_expected
  FROM drafts.vetoes v
  JOIN drafts.picks p ON p.id = v.target_pick_id
  JOIN drafts.draft_parts dp ON dp.id = p.draft_part_id
  JOIN drafts.drafts d ON d.id = dp.draft_id AND d.is_deleted = false
  WHERE v.is_overridden = false
    AND dp.public_id IN (
      SELECT draft_part_public_id FROM reporting.draft_summaries WHERE is_complete);

  SELECT vetoes_count INTO v_current FROM reporting.site_stats LIMIT 1;
  RAISE NOTICE 'site_stats.vetoes_count: current %, expected %', v_current, v_expected;

  UPDATE reporting.site_stats
  SET vetoes_count = v_expected, updated_at = now()
  WHERE vetoes_count <> v_expected;
END $$;

DROP TABLE IF EXISTS pg_temp._stale_parts;
