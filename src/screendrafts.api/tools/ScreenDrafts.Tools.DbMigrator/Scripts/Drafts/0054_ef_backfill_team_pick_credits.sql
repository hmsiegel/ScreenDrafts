-- One-time data fix: credit each member of the team for historic team-played picks that have
-- no team_pick_credits rows (64 of 65 in prod at 2026-10-03).
-- Source of truth is current drafter_team_drafter membership. That is safe here because every
-- affected team is a fixed pair that played in one to three drafts; the Screen Drafts Legends
-- team (8 members, already credited on its one pick) is untouched because it has no uncredited picks.
-- Idempotent: only picks with zero credits are touched, and the unique index guards duplicates.
-- Run on prod and dev. Must run before 0020_crossschema_backfill_record_book_facts.sql.

START TRANSACTION;

INSERT INTO drafts.team_pick_credits (id, target_pick_id, drafter_id_value)
SELECT
  gen_random_uuid(),
  p.id,
  tm.drafter_id
FROM drafts.picks p
JOIN drafts.drafter_team_drafter tm
  ON tm.drafter_team_id = p.played_by_participant_id_value
WHERE p.played_by_participant_kind_value = 1
  AND NOT EXISTS (
    SELECT 1 FROM drafts.team_pick_credits c WHERE c.target_pick_id = p.id);

-- Expect 0 after the insert.
SELECT COUNT(*) AS team_picks_still_without_credits
FROM drafts.picks p
WHERE p.played_by_participant_kind_value = 1
  AND NOT EXISTS (
    SELECT 1 FROM drafts.team_pick_credits c WHERE c.target_pick_id = p.id);

COMMIT;