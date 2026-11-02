-- One-time data fix: Maureen Lee Lenker's veto override chain, per the wiki.
-- She received one veto override, on Shakespeare: Part I Mini-Mega, and used it on Alfred Hitchcock
-- Mega Draft part 3. Nothing rolls over after that.
--
-- The data has nine awards instead of one (Humphrey Bogart, '30s Warner Bros, Mothers & Daughters,
-- Natalie Wood, Jane Austen, Directors' Final Films, Errol Flynn & Olivia de Havilland, Sidney Poitier,
-- plus the real one on Shakespeare Part I). It also carries one override into drafts before Shakespeare
-- Part I, and one past Hitchcock Part 3, which spent it.
-- After the fix the chain reads: 0 in and 1 awarded on Shakespeare Part I; 1 rolling in through every
-- draft up to Hitchcock Part 3; 1 used there; 0 afterwards. Her Hitchcock counter (1) already matches
-- its override row and does not change.
--
-- Idempotent: a row already in its after-state is skipped. A row matching neither state raises, and the
-- whole script rolls back. Drafts schema only; Reporting read models are not touched. Run on prod and dev.

START TRANSACTION;

DO $$
DECLARE
  r       record;
  v_row   drafts.draft_part_participants%ROWTYPE;
  v_id    uuid;
  v_n     int;
  v_fixed int := 0;
BEGIN
  SELECT COUNT(*) INTO v_n FROM drafts.people WHERE display_name ILIKE '%Lenker%';
  IF v_n <> 1 THEN
    RAISE EXCEPTION 'Maureen chain fix: expected 1 person matching Lenker, found %', v_n;
  END IF;

  FOR r IN
    SELECT * FROM (VALUES
      -- title, part_index, before: rolling_in, awarded, used  ->  after: rolling_in, awarded, used
      ('Humphrey Bogart',                                1, 0, 1, 0,   0, 0, 0),
      ('''30s Warner Bros',                              1, 1, 1, 0,   0, 0, 0),
      ('Mothers & Daughters',                            1, 1, 1, 0,   0, 0, 0),
      ('Shakespeare: Part I Mini-Mega',                  1, 1, 1, 0,   0, 1, 0),
      ('Natalie Wood',                                   1, 1, 1, 0,   1, 0, 0),
      ('Jane Austen',                                    1, 1, 1, 0,   1, 0, 0),
      ('Directors'' Final Films',                        1, 1, 1, 0,   1, 0, 0),
      ('Errol Flynn & Olivia de Havilland Super Draft',  1, 0, 1, 0,   0, 0, 0),
      ('Movies About Theatre mini-Mega',                 1, 1, 0, 0,   0, 0, 0),
      ('Sidney Poitier mini-Mega',                       1, 1, 1, 0,   0, 0, 0)
    ) AS t(title, part_index, b_roll, b_aw, b_used, a_roll, a_aw, a_used)
  LOOP
    SELECT COUNT(*) INTO v_n
    FROM drafts.draft_part_participants dpp
    JOIN drafts.drafters dr    ON dr.id = dpp.participant_id_value
    JOIN drafts.people pe      ON pe.id = dr.person_id
    JOIN drafts.draft_parts dp ON dp.id = dpp.draft_part_id
    JOIN drafts.drafts d       ON d.id = dp.draft_id
    WHERE dpp.participant_kind_value = 0
      AND pe.display_name ILIKE '%Lenker%'
      AND d.title = r.title AND dp.part_index = r.part_index;
    IF v_n <> 1 THEN
      RAISE EXCEPTION 'Maureen chain fix: expected 1 row for "%" part %, found %', r.title, r.part_index, v_n;
    END IF;

    SELECT dpp.id INTO v_id
    FROM drafts.draft_part_participants dpp
    JOIN drafts.drafters dr    ON dr.id = dpp.participant_id_value
    JOIN drafts.people pe      ON pe.id = dr.person_id
    JOIN drafts.draft_parts dp ON dp.id = dpp.draft_part_id
    JOIN drafts.drafts d       ON d.id = dp.draft_id
    WHERE dpp.participant_kind_value = 0
      AND pe.display_name ILIKE '%Lenker%'
      AND d.title = r.title AND dp.part_index = r.part_index;

    SELECT * INTO v_row FROM drafts.draft_part_participants WHERE id = v_id;

    IF v_row.veto_overrides_rolling_in = r.a_roll AND v_row.awarded_veto_overrides = r.a_aw
       AND v_row.veto_overrides_used = r.a_used THEN
      RAISE NOTICE 'Maureen chain fix: "%" part % already corrected, skipped.', r.title, r.part_index;
      CONTINUE;
    END IF;

    IF NOT (v_row.veto_overrides_rolling_in = r.b_roll AND v_row.awarded_veto_overrides = r.b_aw
            AND v_row.veto_overrides_used = r.b_used) THEN
      RAISE EXCEPTION 'Maureen chain fix: "%" part % has rolling_in %, awarded %, used %; expected %/%/%',
        r.title, r.part_index, v_row.veto_overrides_rolling_in, v_row.awarded_veto_overrides,
        v_row.veto_overrides_used, r.b_roll, r.b_aw, r.b_used;
    END IF;

    UPDATE drafts.draft_part_participants
    SET veto_overrides_rolling_in = r.a_roll, awarded_veto_overrides = r.a_aw,
        veto_overrides_used = r.a_used
    WHERE id = v_id;
    v_fixed := v_fixed + 1;
  END LOOP;

  RAISE NOTICE 'Maureen chain fix: % row(s) corrected.', v_fixed;
END $$;

COMMIT;
