-- One-time data fix: Clark Collis received a bonus veto on 21st Century British Isles Indie mini-Mega
-- (wiki). The draft has no board position for him, so no flag could back the award and the award rebuild in 0064
-- reset his stored award to 0.
--
-- Sets awarded_vetoes = 1 on his participant row (and has_bonus_veto on his board position, if the part
-- ever has a board with one for him), then carries the veto down his later Global-scope parts with the
-- rule of 0064: a part rolls 1 out when starting + rolling_in + awarded - used >= 1. Vetoes paid from the
-- fungible pool do not count as used. starting_vetoes and vetoes_used are never changed.
--
-- Idempotent: a second run changes nothing. Raises, and rolls back, when the person, part, participant row
-- or board position is not found exactly once, or when his row is not in the expected state.
-- Drafts schema only; Reporting is untouched.

START TRANSACTION;

DO $$
DECLARE
  v_n         int;
  v_drafter   uuid;
  v_part      uuid;
  v_dpp       drafts.draft_part_participants%ROWTYPE;
  v_pos_id    uuid;
  v_pos_flag  boolean;
  v_boards    int;
  v_rn        int;
  v_carry     int;
  v_changed   int := 0;
  r           record;
BEGIN
  SELECT COUNT(*) INTO v_n FROM drafts.people WHERE display_name ILIKE '%Collis%';
  IF v_n <> 1 THEN
    RAISE EXCEPTION 'Clark fix: expected 1 person matching Collis, found %', v_n;
  END IF;

  SELECT dr.id INTO v_drafter
  FROM drafts.people pe JOIN drafts.drafters dr ON dr.person_id = pe.id
  WHERE pe.display_name ILIKE '%Collis%';
  IF v_drafter IS NULL THEN
    RAISE EXCEPTION 'Clark fix: no drafter profile for Collis';
  END IF;

  SELECT COUNT(*) INTO v_n
  FROM drafts.draft_parts dp JOIN drafts.drafts d ON d.id = dp.draft_id
  WHERE d.title = '21st Century British Isles Indie mini-Mega' AND dp.part_index = 1;
  IF v_n <> 1 THEN
    RAISE EXCEPTION 'Clark fix: expected 1 part for 21st Century British Isles Indie mini-Mega part 1, found %', v_n;
  END IF;
  SELECT dp.id INTO v_part
  FROM drafts.draft_parts dp JOIN drafts.drafts d ON d.id = dp.draft_id
  WHERE d.title = '21st Century British Isles Indie mini-Mega' AND dp.part_index = 1;

  SELECT COUNT(*) INTO v_n FROM drafts.draft_part_participants
  WHERE draft_part_id = v_part AND participant_id_value = v_drafter AND participant_kind_value = 0;
  IF v_n <> 1 THEN
    RAISE EXCEPTION 'Clark fix: expected 1 participant row, found %', v_n;
  END IF;
  SELECT * INTO v_dpp FROM drafts.draft_part_participants
  WHERE draft_part_id = v_part AND participant_id_value = v_drafter AND participant_kind_value = 0;

  IF v_dpp.awarded_vetoes NOT IN (0, 1) THEN
    RAISE EXCEPTION 'Clark fix: row has awarded vetoes %; expected 0 or 1', v_dpp.awarded_vetoes;
  END IF;

  -- His board position. A part with no board, or a board with no position assigned to him (this draft's
  -- board has none), has nothing to flag.
  SELECT COUNT(*) INTO v_boards
  FROM drafts.draft_positions pos JOIN drafts.game_boards gb ON gb.id = pos.game_board_id
  WHERE gb.draft_part_id = v_part AND pos.assigned_to_id = v_drafter AND pos.assigned_to_kind = 0;
  IF v_boards = 0 THEN
    RAISE NOTICE 'Clark fix: no board position is assigned to him in this part, so there is nothing to flag.';
  ELSE
    SELECT COUNT(*) INTO v_n
    FROM drafts.draft_positions pos JOIN drafts.game_boards gb ON gb.id = pos.game_board_id
    WHERE gb.draft_part_id = v_part AND pos.assigned_to_id = v_drafter AND pos.assigned_to_kind = 0;
    IF v_n <> 1 THEN
      RAISE EXCEPTION 'Clark fix: expected 1 board position assigned to him, found %', v_n;
    END IF;
    SELECT pos.id, pos.has_bonus_veto INTO v_pos_id, v_pos_flag
    FROM drafts.draft_positions pos JOIN drafts.game_boards gb ON gb.id = pos.game_board_id
    WHERE gb.draft_part_id = v_part AND pos.assigned_to_id = v_drafter AND pos.assigned_to_kind = 0;

    IF NOT v_pos_flag THEN
      UPDATE drafts.draft_positions SET has_bonus_veto = true WHERE id = v_pos_id;
      v_changed := v_changed + 1;
    END IF;
  END IF;
  IF v_dpp.awarded_vetoes <> 1 THEN
    UPDATE drafts.draft_part_participants SET awarded_vetoes = 1 WHERE id = v_dpp.id;
    v_changed := v_changed + 1;
  END IF;

  -- Carry down his later parts (completed, Global-scope or no series, release order).
  v_carry := CASE WHEN v_dpp.starting_vetoes + v_dpp.vetoes_rolling_in + 1
                  - GREATEST(v_dpp.vetoes_used,
                             (SELECT COUNT(*)::int FROM drafts.vetoes v
                              WHERE v.issued_by_participant_id = v_dpp.id AND NOT v.spent_from_fungible_pool)) >= 1
             THEN 1 ELSE 0 END;

  SELECT rn INTO v_rn FROM (
    SELECT dpp.id AS dpp_id,
           ROW_NUMBER() OVER (ORDER BY MIN(rl.release_date) ASC NULLS LAST, dp.created_at_utc ASC) AS rn
    FROM drafts.draft_part_participants dpp
    JOIN drafts.draft_parts dp ON dp.id = dpp.draft_part_id
    JOIN drafts.drafts d       ON d.id = dp.draft_id
    LEFT JOIN drafts.series s  ON s.id = d.series_id
    LEFT JOIN drafts.draft_releases rl ON rl.part_id = dp.id
    WHERE dpp.participant_id_value = v_drafter AND dpp.participant_kind_value = 0
      AND dp.status = 3 AND (s.id IS NULL OR s.continuity_scope = 2)
    GROUP BY dpp.id, dp.created_at_utc
  ) x WHERE x.dpp_id = v_dpp.id;
  IF v_rn IS NULL THEN
    RAISE EXCEPTION 'Clark fix: the 21st Century part is not in his completed Global chain';
  END IF;

  FOR r IN
    SELECT x.dpp_id, x.st, x.rolling_in, x.awarded, x.used
    FROM (
      SELECT dpp.id AS dpp_id,
             dpp.vetoes_rolling_in AS rolling_in,
             dpp.awarded_vetoes    AS awarded,
             dpp.starting_vetoes           AS st,
             GREATEST(dpp.vetoes_used,
                      (SELECT COUNT(*)::int FROM drafts.vetoes v
                       WHERE v.issued_by_participant_id = dpp.id AND NOT v.spent_from_fungible_pool)) AS used,
             ROW_NUMBER() OVER (ORDER BY MIN(rl.release_date) ASC NULLS LAST, dp.created_at_utc ASC) AS rn
      FROM drafts.draft_part_participants dpp
      JOIN drafts.draft_parts dp ON dp.id = dpp.draft_part_id
      JOIN drafts.drafts d       ON d.id = dp.draft_id
      LEFT JOIN drafts.series s  ON s.id = d.series_id
      LEFT JOIN drafts.draft_releases rl ON rl.part_id = dp.id
      WHERE dpp.participant_id_value = v_drafter AND dpp.participant_kind_value = 0
        AND dp.status = 3 AND (s.id IS NULL OR s.continuity_scope = 2)
      GROUP BY dpp.id, dp.created_at_utc, dpp.vetoes_rolling_in, dpp.starting_vetoes,
               dpp.awarded_vetoes, dpp.vetoes_used
    ) x
    WHERE x.rn > v_rn
    ORDER BY x.rn
  LOOP
    IF r.rolling_in <> v_carry THEN
      UPDATE drafts.draft_part_participants SET vetoes_rolling_in = v_carry WHERE id = r.dpp_id;
      v_changed := v_changed + 1;
    END IF;
    v_carry := CASE WHEN r.st + v_carry + r.awarded - r.used >= 1 THEN 1 ELSE 0 END;
  END LOOP;

  RAISE NOTICE 'Clark fix: % change(s) made; veto rolling out of his latest Global part: %', v_changed, v_carry;
END $$;

COMMIT;
