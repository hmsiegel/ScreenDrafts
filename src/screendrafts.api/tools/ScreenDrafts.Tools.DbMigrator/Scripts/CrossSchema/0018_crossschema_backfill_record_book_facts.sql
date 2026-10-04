-- Backfills reporting.pick_facts, veto_facts and pick_credit_facts from the Drafts schema.
-- Sanctioned cross-schema script (runs as the superuser). Idempotent: it replaces the facts
-- of every part listed in reporting.draft_summaries, so a re-run converges to the same rows.
-- Requires the 0007 Reporting migration (the three fact tables) and the two Drafts data-fix
-- scripts (0018 team credits, 0019 orphan pick) to have run first.
-- "Completed part" = has a reporting.draft_summaries row, which the live pipeline upserts
-- when a part completes. Soft-deleted drafts are excluded.
--
-- Participant kinds: 0 = drafter, 1 = team, 2 = community.
-- No explicit transaction: the DbMigrator cross-schema runner wraps every script in one, and an
-- inner COMMIT breaks its journal insert ("Transaction is already completed").


DELETE FROM reporting.pick_credit_facts
WHERE draft_part_public_id IN (SELECT draft_part_public_id FROM reporting.draft_summaries);

DELETE FROM reporting.veto_facts
WHERE draft_part_public_id IN (SELECT draft_part_public_id FROM reporting.draft_summaries);

DELETE FROM reporting.pick_facts
WHERE draft_part_public_id IN (SELECT draft_part_public_id FROM reporting.draft_summaries);

-- 1. pick_facts
INSERT INTO reporting.pick_facts
  (id, draft_id, draft_public_id, draft_part_public_id, part_index, draft_title,
   draft_type, series_name, canonical_policy, sub_draft_index, position,
   media_public_id, media_title, played_by_kind, played_by_id_value,
   played_by_public_id, played_by_name, veto_count, was_vetoed,
   was_veto_overridden, was_commissioner_overridden, recorded_at_utc)
SELECT
  p.id,
  d.id,
  d.public_id,
  dp.public_id,
  dp.part_index,
  d.title,
  CASE dp.draft_type
    WHEN 0 THEN 'Standard'
    WHEN 1 THEN 'MiniMega'
    WHEN 2 THEN 'Mega'
    WHEN 3 THEN 'Super'
    WHEN 4 THEN 'MiniSuper'
    WHEN 5 THEN 'SpeedDraft'
    ELSE dp.draft_type::text
  END,
  s.name,
  s.canonical_policy,
  sd.index,
  p.position,
  m.public_id,
  m.movie_title,
  p.played_by_participant_kind_value,
  p.played_by_participant_id_value,
  CASE p.played_by_participant_kind_value
    WHEN 0 THEN dr.public_id
    WHEN 1 THEN t.public_id
    ELSE NULL
  END,
  CASE p.played_by_participant_kind_value
    WHEN 0 THEN COALESCE(
      NULLIF(pe.display_name, ''),
      NULLIF(TRIM(CONCAT_WS(' ', pe.first_name, pe.last_name)), ''),
      'Unknown drafter')
    WHEN 1 THEN COALESCE(t.name, 'Unknown team')
    ELSE 'Community'
  END,
  (SELECT COUNT(*) FROM drafts.vetoes v WHERE v.target_pick_id = p.id)::int,
  EXISTS (SELECT 1 FROM drafts.vetoes v WHERE v.target_pick_id = p.id),
  COALESCE((
    SELECT v.is_overridden
    FROM drafts.vetoes v
    WHERE v.target_pick_id = p.id
    ORDER BY v.sequence DESC
    LIMIT 1), false),
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
WHERE dp.public_id IN (SELECT draft_part_public_id FROM reporting.draft_summaries);

-- 2. veto_facts
INSERT INTO reporting.veto_facts
  (id, pick_id, draft_id, draft_part_public_id, sequence, issued_by_kind,
   issued_by_id_value, issued_by_public_id, issued_by_name, is_overridden,
   overridden_by_kind, overridden_by_id_value, overridden_by_public_id,
   overridden_by_name, is_self_veto, recorded_at_utc)
SELECT
  v.id,
  p.id,
  d.id,
  dp.public_id,
  v.sequence,
  ip.participant_kind_value,
  ip.participant_id_value,
  CASE ip.participant_kind_value WHEN 0 THEN idr.public_id WHEN 1 THEN it.public_id ELSE NULL END,
  CASE ip.participant_kind_value
    WHEN 0 THEN COALESCE(
      NULLIF(ipe.display_name, ''),
      NULLIF(TRIM(CONCAT_WS(' ', ipe.first_name, ipe.last_name)), ''),
      'Unknown drafter')
    WHEN 1 THEN COALESCE(it.name, 'Unknown team')
    ELSE 'Community'
  END,
  v.is_overridden,
  op.participant_kind_value,
  op.participant_id_value,
  CASE op.participant_kind_value WHEN 0 THEN odr.public_id WHEN 1 THEN ot.public_id ELSE NULL END,
  CASE
    WHEN op.id IS NULL THEN NULL
    WHEN op.participant_kind_value = 0 THEN COALESCE(
      NULLIF(ope.display_name, ''),
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
LEFT JOIN drafts.drafters idr
  ON ip.participant_kind_value = 0 AND idr.id = ip.participant_id_value
LEFT JOIN drafts.people ipe ON ipe.id = idr.person_id
LEFT JOIN drafts.drafter_teams it
  ON ip.participant_kind_value = 1 AND it.id = ip.participant_id_value
LEFT JOIN drafts.veto_overrides vo ON vo.veto_id = v.id
LEFT JOIN drafts.draft_part_participants op ON op.id = vo.issued_by_participant_id
LEFT JOIN drafts.drafters odr
  ON op.participant_kind_value = 0 AND odr.id = op.participant_id_value
LEFT JOIN drafts.people ope ON ope.id = odr.person_id
LEFT JOIN drafts.drafter_teams ot
  ON op.participant_kind_value = 1 AND ot.id = op.participant_id_value
WHERE dp.public_id IN (SELECT draft_part_public_id FROM reporting.draft_summaries);

-- 3. pick_credit_facts: solo drafter picks credit the player; team picks credit the
--    snapshotted members; community picks credit no one.
INSERT INTO reporting.pick_credit_facts
  (id, pick_id, draft_id, draft_part_public_id, drafter_id_value,
   drafter_public_id, drafter_name, recorded_at_utc)
SELECT
  gen_random_uuid(),
  c.pick_id,
  c.draft_id,
  c.draft_part_public_id,
  dr.id,
  dr.public_id,
  COALESCE(
    NULLIF(pe.display_name, ''),
    NULLIF(TRIM(CONCAT_WS(' ', pe.first_name, pe.last_name)), ''),
    'Unknown drafter'),
  now()
FROM (
  SELECT pf.id AS pick_id, pf.draft_id, pf.draft_part_public_id, pf.played_by_id_value AS drafter_id
  FROM reporting.pick_facts pf
  WHERE pf.played_by_kind = 0
    AND pf.draft_part_public_id IN (SELECT draft_part_public_id FROM reporting.draft_summaries)
  UNION
  SELECT pf.id, pf.draft_id, pf.draft_part_public_id, tc.drafter_id_value
  FROM reporting.pick_facts pf
  JOIN drafts.team_pick_credits tc ON tc.target_pick_id = pf.id
  WHERE pf.played_by_kind = 1
    AND pf.draft_part_public_id IN (SELECT draft_part_public_id FROM reporting.draft_summaries)
) c
JOIN drafts.drafters dr ON dr.id = c.drafter_id
JOIN drafts.people pe ON pe.id = dr.person_id;