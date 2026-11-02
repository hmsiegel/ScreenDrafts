-- One-time data fix: rebuild veto_overrides_rolling_in down every drafter's chain of solo parts.
-- Historical parts were stored without the override that the previous part should have rolled
-- in, so drafters who earned an override and never spent it show no rollover override
-- (e.g. Adam B. Vary, who has held one since the Star Wars Super Draft). The next live part
-- also reads its rolling-in from the latest stored part, so the gap would carry forward.
--
-- Rule, same as SetDraftPartStatusCommandHandler.ApplyRolloversAsync: a part rolls 1 override
-- out when rolling_in + awarded - used >= 1. Chain = completed (status 3) kind-0 rows of parts
-- in Global-scope series, or with no series, in release order (MIN release date over all
-- channels, then created_at_utc). Team rows and other scopes are never touched.
-- "Used" is the larger of the counter and the override rows the participant issued, so an
-- override that was spent but never counted is not rolled over again.
--
-- Fill-only: a stored rolling-in of 1 is never lowered, and the first part of a chain keeps its
-- stored value. The script only raises 0 to 1, where the rule says the override carried over.
-- Idempotent: a second run changes nothing. Veto rollover and every other column are untouched.

START TRANSACTION;

DO $$
DECLARE
  v_rows    int;
  v_changed int;
  v_people  int;
BEGIN
  CREATE TEMP TABLE _override_chain ON COMMIT DROP AS
  WITH RECURSIVE parts AS (
    SELECT dpp.participant_id_value AS drafter_id,
           dpp.id                   AS dpp_id,
           MIN(r.release_date)      AS rel,
           dp.created_at_utc,
           dpp.veto_overrides_rolling_in AS stored_rolling_in,
           dpp.awarded_veto_overrides,
           GREATEST(dpp.veto_overrides_used,
                    (SELECT COUNT(*)::int FROM drafts.veto_overrides vo
                     WHERE vo.issued_by_participant_id = dpp.id)) AS used
    FROM drafts.draft_part_participants dpp
    JOIN drafts.draft_parts dp ON dp.id = dpp.draft_part_id
    JOIN drafts.drafts d       ON d.id = dp.draft_id
    LEFT JOIN drafts.series s  ON s.id = d.series_id
    LEFT JOIN drafts.draft_releases r ON r.part_id = dp.id
    WHERE dpp.participant_kind_value = 0
      AND dp.status = 3
      AND (s.id IS NULL OR s.continuity_scope = 2) -- 2 = ContinuityScope.Global
    GROUP BY dpp.participant_id_value, dpp.id, dp.created_at_utc,
             dpp.veto_overrides_rolling_in, dpp.awarded_veto_overrides, dpp.veto_overrides_used
  ),
  numbered AS (
    SELECT *, ROW_NUMBER() OVER (
             PARTITION BY drafter_id ORDER BY rel ASC NULLS LAST, created_at_utc ASC) AS rn
    FROM parts
  ),
  chain AS (
    SELECT n.*, n.stored_rolling_in AS new_rolling_in
    FROM numbered n
    WHERE n.rn = 1
    UNION ALL
    SELECT n.*,
           GREATEST(n.stored_rolling_in,
                    CASE WHEN c.new_rolling_in + c.awarded_veto_overrides - c.used >= 1 THEN 1 ELSE 0 END)
    FROM chain c
    JOIN numbered n ON n.drafter_id = c.drafter_id AND n.rn = c.rn + 1
  )
  SELECT dpp_id, drafter_id, stored_rolling_in, new_rolling_in FROM chain;

  SELECT COUNT(*) INTO v_rows FROM _override_chain;
  IF v_rows = 0 THEN
    RAISE EXCEPTION 'Override rollover rebuild: no completed Global-scope solo parts found';
  END IF;

  IF EXISTS (SELECT 1 FROM _override_chain WHERE new_rolling_in NOT IN (0, 1) OR new_rolling_in < stored_rolling_in) THEN
    RAISE EXCEPTION 'Override rollover rebuild: a rebuilt value is outside 0/1 or below the stored value';
  END IF;

  UPDATE drafts.draft_part_participants dpp
  SET veto_overrides_rolling_in = c.new_rolling_in
  FROM _override_chain c
  WHERE dpp.id = c.dpp_id
    AND dpp.veto_overrides_rolling_in <> c.new_rolling_in;

  GET DIAGNOSTICS v_changed = ROW_COUNT;

  SELECT COUNT(DISTINCT c.drafter_id) INTO v_people
  FROM _override_chain c
  JOIN drafts.draft_part_participants dpp ON dpp.id = c.dpp_id
  WHERE dpp.veto_overrides_rolling_in = 1 AND c.stored_rolling_in = 0;

  RAISE NOTICE 'Override rollover rebuild: % parts in chains, % rolling-in values raised to 1 (% drafters)',
    v_rows, v_changed, v_people;
END $$;

COMMIT;
