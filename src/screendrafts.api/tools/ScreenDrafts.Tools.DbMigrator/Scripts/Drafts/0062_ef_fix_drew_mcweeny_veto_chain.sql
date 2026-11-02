-- One-time data fix: Drew McWeeny's veto counters, per the wiki spreadsheet.
-- Awards: only James Bond Super Draft part 2, Steven Spielberg Super Draft and Vietnam War mini-Mega.
-- Used: "1980 mini-Mega" 1 veto, "Joe Dante" 2 vetoes.
--
-- The data has four extra awards (X / NC-17 / Unrated, Francis Ford Coppola part 2, John Patrick Shanley,
-- '70s Comedy mini-Mega) and a starting_vetoes of 2 on "1980 mini-Mega" and "Joe Dante" (always 1 elsewhere).
-- The extra Coppola award made Shanley roll in a veto that the wiki does not show, so that row loses it.
-- The two vetoes_used counters sit at 0 while the veto rows say 1 and 2; they now match the rows.
-- Result: Drew's chain ends "Joe Dante" at 1 + 1 - 2 = 0 unspent, so no rollover veto is left.
--
-- Idempotent: a row already in its after-state is skipped. A row matching neither state raises, and the
-- whole script rolls back. Drafts schema only; Reporting read models are not touched. Run on prod and dev.

START TRANSACTION;

DO $$
DECLARE
  r        record;
  v_row    drafts.draft_part_participants%ROWTYPE;
  v_id     uuid;
  v_n      int;
  v_fixed  int := 0;
BEGIN
  FOR r IN
    SELECT * FROM (VALUES
      -- title, part_index, before: start, rolling_in, awarded, used  ->  after: start, rolling_in, awarded, used
      ('1980 mini-Mega',                     1, 2, 1, 0, 0,   1, 1, 0, 1),
      ('Joe Dante',                          1, 2, 1, 0, 0,   1, 1, 0, 2),
      ('X / NC-17 / Unrated',                1, 1, 1, 1, 0,   1, 1, 0, 0),
      ('Francis Ford Coppola Super Draft',   2, 1, 1, 1, 2,   1, 1, 0, 2),
      ('John Patrick Shanley Super Draft',   1, 1, 1, 1, 0,   1, 0, 0, 0),
      ('''70s Comedy mini-Mega',             1, 1, 1, 1, 1,   1, 1, 0, 1)
    ) AS t(title, part_index, b_start, b_roll, b_aw, b_used, a_start, a_roll, a_aw, a_used)
  LOOP
    SELECT COUNT(*) INTO v_n
    FROM drafts.draft_part_participants dpp
    JOIN drafts.drafters dr    ON dr.id = dpp.participant_id_value
    JOIN drafts.people pe      ON pe.id = dr.person_id
    JOIN drafts.draft_parts dp ON dp.id = dpp.draft_part_id
    JOIN drafts.drafts d       ON d.id = dp.draft_id
    WHERE dpp.participant_kind_value = 0
      AND pe.first_name = 'Drew' AND pe.last_name = 'McWeeny'
      AND d.title = r.title AND dp.part_index = r.part_index;
    IF v_n <> 1 THEN
      RAISE EXCEPTION 'Drew chain fix: expected 1 row for "%" part %, found %', r.title, r.part_index, v_n;
    END IF;

    SELECT dpp.id INTO v_id
    FROM drafts.draft_part_participants dpp
    JOIN drafts.drafters dr    ON dr.id = dpp.participant_id_value
    JOIN drafts.people pe      ON pe.id = dr.person_id
    JOIN drafts.draft_parts dp ON dp.id = dpp.draft_part_id
    JOIN drafts.drafts d       ON d.id = dp.draft_id
    WHERE dpp.participant_kind_value = 0
      AND pe.first_name = 'Drew' AND pe.last_name = 'McWeeny'
      AND d.title = r.title AND dp.part_index = r.part_index;

    SELECT * INTO v_row FROM drafts.draft_part_participants WHERE id = v_id;

    IF v_row.starting_vetoes = r.a_start AND v_row.vetoes_rolling_in = r.a_roll
       AND v_row.awarded_vetoes = r.a_aw AND v_row.vetoes_used = r.a_used THEN
      RAISE NOTICE 'Drew chain fix: "%" part % already corrected, skipped.', r.title, r.part_index;
      CONTINUE;
    END IF;

    IF NOT (v_row.starting_vetoes = r.b_start AND v_row.vetoes_rolling_in = r.b_roll
            AND v_row.awarded_vetoes = r.b_aw AND v_row.vetoes_used = r.b_used) THEN
      RAISE EXCEPTION 'Drew chain fix: "%" part % has start %, rolling_in %, awarded %, used %; expected %/%/%/%',
        r.title, r.part_index, v_row.starting_vetoes, v_row.vetoes_rolling_in, v_row.awarded_vetoes,
        v_row.vetoes_used, r.b_start, r.b_roll, r.b_aw, r.b_used;
    END IF;

    UPDATE drafts.draft_part_participants
    SET starting_vetoes = r.a_start, vetoes_rolling_in = r.a_roll,
        awarded_vetoes = r.a_aw, vetoes_used = r.a_used
    WHERE id = v_id;
    v_fixed := v_fixed + 1;
  END LOOP;

  RAISE NOTICE 'Drew chain fix: % row(s) corrected.', v_fixed;
END $$;

COMMIT;
