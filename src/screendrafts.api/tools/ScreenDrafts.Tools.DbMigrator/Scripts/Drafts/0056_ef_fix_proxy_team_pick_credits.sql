-- One-time data fix: Ryan Marker drafted the Gene Hackman mini-Mega as a proxy for Roxana Hadadi,
-- who could not attend. Roxana was the drafter, so Ryan must not be credited with those picks or
-- count that draft as an appearance. The 0054 credit backfill credited every current team member,
-- including Ryan, so remove his credits on that team's picks. Roxana's credits stay.
-- Idempotent. Run on prod and dev. Must run before the facts backfill; on a database where the
-- backfill already ran (dev), re-run the backfill script by hand afterwards. It is idempotent.
-- Team: "Ryan Marker & Roxana Hadadi" (public id dt_Y3TRJ3TNLnXRJQf).

DELETE FROM drafts.team_pick_credits c
USING drafts.picks p, drafts.drafters dr, drafts.people pe
WHERE c.target_pick_id = p.id
  AND dr.id = c.drafter_id_value
  AND pe.id = dr.person_id
  AND p.played_by_participant_kind_value = 1
  AND p.played_by_participant_id_value =
      (SELECT t.id FROM drafts.drafter_teams t WHERE t.public_id = 'dt_Y3TRJ3TNLnXRJQf')
  AND COALESCE(NULLIF(pe.display_name, ''), TRIM(CONCAT_WS(' ', pe.first_name, pe.last_name)))
      = 'Ryan Marker';

-- Expect one credit row per pick (Roxana only): 3 picks, 3 credits, none for Ryan.
SELECT p.position,
       COALESCE(NULLIF(pe.display_name, ''), TRIM(CONCAT_WS(' ', pe.first_name, pe.last_name))) AS credited
FROM drafts.picks p
JOIN drafts.team_pick_credits c ON c.target_pick_id = p.id
JOIN drafts.drafters dr ON dr.id = c.drafter_id_value
JOIN drafts.people pe ON pe.id = dr.person_id
WHERE p.played_by_participant_kind_value = 1
  AND p.played_by_participant_id_value =
      (SELECT t.id FROM drafts.drafter_teams t WHERE t.public_id = 'dt_Y3TRJ3TNLnXRJQf')
ORDER BY p.position;