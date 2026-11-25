-- One-time data fix: Kenny Neibart's awarded vetoes in the Matt vs Ben mini-Mega Draft
-- (draft part dp_3Pq6pZDUv5MsDP4).
--
-- Editing the draft re-sent the draft positions, which recreated them empty without revoking the
-- awards the old assignments had granted. Assigning Kenny's bonus-veto position again then
-- granted the veto a second time: awarded_vetoes is 2 where he holds one bonus-veto position.
-- That left him with 3 vetoes for the draft (1 starting + 2 awarded) and a phantom veto rolling
-- out to his next draft.
--
-- Fix: awarded_vetoes becomes the number of bonus-veto positions assigned to him in this part.
-- Expected result: total 2, used 2, rolling out 0.
--
-- PROD only in effect: this draft was seeded in PROD, so environments without the part (DEV,
-- TEST) skip with a notice instead of failing and blocking later Drafts scripts.
-- Idempotent: skipped when awarded_vetoes already matches the position count. Once the part
-- exists, raises, and rolls back, when the drafter is not found exactly once, or awarded_vetoes
-- is neither the expected 2 nor the already-correct count.
-- Drafts schema only.

START TRANSACTION;

DO $$
DECLARE
  c_part_public_id CONSTANT text := 'dp_3Pq6pZDUv5MsDP4';
  c_drafter_name   CONSTANT text := 'Kenny Neibart';
  v_part_id        uuid;
  v_dpp_id         uuid;
  v_awarded        int;
  v_bonus_count    int;
BEGIN
  SELECT dp.id INTO v_part_id
  FROM drafts.draft_parts dp
  WHERE dp.public_id = c_part_public_id;

  IF NOT FOUND THEN
    RAISE NOTICE 'Kenny Neibart: draft part % not in this database, skipped.', c_part_public_id;
    RETURN;
  END IF;

  -- STRICT raises when a query returns zero or several rows.
  SELECT dpp.id, dpp.awarded_vetoes INTO STRICT v_dpp_id, v_awarded
  FROM drafts.draft_part_participants dpp
  JOIN drafts.drafters dr ON dr.id = dpp.participant_id_value
  JOIN drafts.people pe   ON pe.id = dr.person_id
  WHERE dpp.draft_part_id = v_part_id
    AND dpp.participant_kind_value = 0
    AND pe.display_name = c_drafter_name;

  SELECT COUNT(*) INTO v_bonus_count
  FROM drafts.draft_positions pos
  JOIN drafts.game_boards gb ON gb.id = pos.game_board_id
  JOIN drafts.draft_part_participants dpp ON dpp.id = v_dpp_id
  WHERE gb.draft_part_id = v_part_id
    AND pos.assigned_to_id = dpp.participant_id_value
    AND pos.has_bonus_veto;

  IF v_awarded = v_bonus_count THEN
    RAISE NOTICE 'Kenny Neibart: awarded_vetoes already % (matches bonus positions), skipped.', v_awarded;
    RETURN;
  END IF;

  IF v_awarded <> 2 THEN
    RAISE EXCEPTION 'Kenny Neibart: expected awarded_vetoes 2 before the fix, found % (bonus positions: %).',
      v_awarded, v_bonus_count;
  END IF;

  UPDATE drafts.draft_part_participants
  SET awarded_vetoes = v_bonus_count
  WHERE id = v_dpp_id;

  RAISE NOTICE 'Kenny Neibart: awarded_vetoes % -> %.', v_awarded, v_bonus_count;
END $$;

COMMIT;
