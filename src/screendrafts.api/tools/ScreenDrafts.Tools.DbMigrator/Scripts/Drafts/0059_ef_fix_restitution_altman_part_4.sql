-- One-time data fix: Restitution award for Ryan Marker on Robert Altman Super Draft, Part 4.
-- Emily St. James vetoed Ryan when she should not have had a veto, so Clay Keller awarded Ryan
-- a bonus veto and a bonus veto override (wiki: Robert Altman Super Draft, Part IV).
-- No app path awards vetoes by hand, so this adds 1 to awarded_vetoes and 1 to awarded_veto_overrides
-- on Ryan's participant row for that part.
--
-- Expected row before: starting 0, rolling_in 1, awarded 1 (the board's bonus-veto position), used 2
-- (vetoes); rolling_in 0, awarded 0, used 0 (overrides).
-- Expected row after:  awarded_vetoes 2, awarded_veto_overrides 1. Both items stay unspent, so they roll
-- into Ryan's next Global-scope draft (cap 1 each). The four later drafts (Ip Man, Lanterns, Speed Drafts
-- XX and XXI) are Patreon-only, scope None or SpeedDrafts, and read nothing from Part 4.
--
-- Idempotent: exits with a notice when the row already shows the after-state. Raises, and rolls back,
-- when the row is not found exactly once or matches neither state. Run on prod and dev.

START TRANSACTION;

DO $$
DECLARE
  v_matches int;
  v_row     drafts.draft_part_participants%ROWTYPE;
BEGIN
  SELECT COUNT(*) INTO v_matches
  FROM drafts.draft_part_participants dpp
  JOIN drafts.drafters dr    ON dr.id = dpp.participant_id_value
  JOIN drafts.people pe      ON pe.id = dr.person_id
  JOIN drafts.draft_parts dp ON dp.id = dpp.draft_part_id
  JOIN drafts.drafts d       ON d.id = dp.draft_id
  WHERE dpp.participant_kind_value = 0
    AND pe.first_name = 'Ryan' AND pe.last_name = 'Marker'
    AND d.title = 'Robert Altman Super Draft'
    AND dp.part_index = 4;

  IF v_matches <> 1 THEN
    RAISE EXCEPTION 'Restitution fix: expected 1 participant row for Ryan Marker on Robert Altman Super Draft part 4, found %', v_matches;
  END IF;

  SELECT dpp.* INTO v_row
  FROM drafts.draft_part_participants dpp
  JOIN drafts.drafters dr    ON dr.id = dpp.participant_id_value
  JOIN drafts.people pe      ON pe.id = dr.person_id
  JOIN drafts.draft_parts dp ON dp.id = dpp.draft_part_id
  JOIN drafts.drafts d       ON d.id = dp.draft_id
  WHERE dpp.participant_kind_value = 0
    AND pe.first_name = 'Ryan' AND pe.last_name = 'Marker'
    AND d.title = 'Robert Altman Super Draft'
    AND dp.part_index = 4;

  IF v_row.starting_vetoes <> 0
     OR v_row.vetoes_rolling_in <> 1
     OR v_row.vetoes_used <> 2
     OR v_row.veto_overrides_rolling_in <> 0
     OR v_row.veto_overrides_used <> 0 THEN
    RAISE EXCEPTION 'Restitution fix: row % no longer matches the checked inventory (starting %, rolling_in %, used %, override rolling_in %, override used %)',
      v_row.id, v_row.starting_vetoes, v_row.vetoes_rolling_in, v_row.vetoes_used,
      v_row.veto_overrides_rolling_in, v_row.veto_overrides_used;
  END IF;

  IF v_row.awarded_vetoes = 2 AND v_row.awarded_veto_overrides = 1 THEN
    RAISE NOTICE 'Restitution fix already applied to participant row %; nothing changed', v_row.id;
  ELSIF v_row.awarded_vetoes = 1 AND v_row.awarded_veto_overrides = 0 THEN
    UPDATE drafts.draft_part_participants
    SET awarded_vetoes = awarded_vetoes + 1,
        awarded_veto_overrides = awarded_veto_overrides + 1
    WHERE id = v_row.id;
    RAISE NOTICE 'Restitution applied to participant row %', v_row.id;
  ELSE
    RAISE EXCEPTION 'Restitution fix: row % has awarded_vetoes % and awarded_veto_overrides % (expected 1 and 0, or 2 and 1)',
      v_row.id, v_row.awarded_vetoes, v_row.awarded_veto_overrides;
  END IF;
END $$;

-- Expect awarded_vetoes 2, awarded_veto_overrides 1.
SELECT dpp.id, dpp.awarded_vetoes, dpp.vetoes_used, dpp.awarded_veto_overrides, dpp.veto_overrides_used
FROM drafts.draft_part_participants dpp
JOIN drafts.drafters dr    ON dr.id = dpp.participant_id_value
JOIN drafts.people pe      ON pe.id = dr.person_id
JOIN drafts.draft_parts dp ON dp.id = dpp.draft_part_id
JOIN drafts.drafts d       ON d.id = dp.draft_id
WHERE dpp.participant_kind_value = 0
  AND pe.first_name = 'Ryan' AND pe.last_name = 'Marker'
  AND d.title = 'Robert Altman Super Draft'
  AND dp.part_index = 4;

COMMIT;
