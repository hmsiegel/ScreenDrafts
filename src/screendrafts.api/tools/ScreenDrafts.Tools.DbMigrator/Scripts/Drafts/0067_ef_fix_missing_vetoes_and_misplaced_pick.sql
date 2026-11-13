-- One-time data fix: seven missing vetoes and one misplaced pick, found by the duplicate-position
-- sweep (two landed picks at the same position in one part). The archive kept both picks of each
-- pair but no veto row, so both showed as landed. Vetoers confirmed by Harlan.
--
-- Missing vetoes (part, pick, vetoer):
--   Disney Attractions Super Draft (ep 323)   Pirates: Dead Man's Chest at #2          Mike Scollins
--   Speed Drafts IV, sub-draft 3              The World Is Not Enough at #6           Ryan Marker
--   The Gambler Franchise mini-Super Draft    Gambler V: Playing for Keeps at #5      Ryan Marker
--   Speed Drafts VI, sub-draft 1              Toy Story of Terror at #3 (play 5)      Ryan Marker
--   Woods Horror mini-Mega Live               Friday the 13th Part VI at #4           Chris Thomas Devlin
--   Dracula mini-Mega Live                    Horror of Dracula at #3                 Larry Fessenden
--   Dracula mini-Mega Live                    Nosferatu: A Symphony of Horror at #2   Emily Hagins
-- Each veto is sequence 1, not overridden. vetoes_used goes up by 1 on the vetoer's participant
-- row, except in Speed Drafts: their rows hold vetoes_used 0 beside 2 to 3 veto rows, so the
-- counter is not maintained there and stays untouched.
--
-- Misplaced pick: Speed Drafts III. Darren Franich's Flatliners sits in sub-draft 3 at #1 with
-- play_order 1. Sub-draft 2 has no #1 pick, and Darren vetoed-and-replayed there (Ryan's Flatliners
-- at #2 was vetoed by Darren). The pick moves to sub-draft 2 with play_order 9, and sub-draft 3's
-- play_order values shift down by one so they start at 1 again.
--
-- Idempotent: a pick that already has a veto is skipped; the misplaced pick is skipped once it is
-- in sub-draft 2. Raises, and rolls back, when any lookup finds zero or several rows, or the
-- sub-drafts are not in the expected before-state.
-- Drafts schema only. Reporting is reconciled by CrossSchema 0023.

START TRANSACTION;

DO $$
DECLARE
  r            record;
  v_part_id    uuid;
  v_draft_type int;
  v_pick_id    uuid;
  v_sub_id     uuid;
  v_dpp_id     uuid;
  v_added      int := 0;
BEGIN
  FOR r IN
    SELECT * FROM (VALUES
      ('dp_tydHKld4FeBkwEg', NULL::int, 'Pirates of the Caribbean: Dead Man''s Chest',        2,  8, 'Mike Scollins'),
      ('dp_37QbK7y93Btzbin', 3,         'The World Is Not Enough',                            6,  2, 'Ryan Marker'),
      ('dp_LfYOQ1e2WHNZ78z', NULL::int, 'Gambler V: Playing for Keeps',                      5,  1, 'Ryan Marker'),
      ('dp_Jy6YtUJgR3iaXlg', 1,         'Toy Story of Terror',                                3,  5, 'Ryan Marker'),
      ('dp_i2FEMLbkwUXQ1sU', NULL::int, 'Friday the 13th Part VI: Jason Lives',              4, 12, 'Chris Thomas Devlin'),
      ('dp_yviuagiq9oVVmBo', NULL::int, 'Horror of Dracula',                                  3,  9, 'Larry Fessenden'),
      ('dp_yviuagiq9oVVmBo', NULL::int, 'Nosferatu: A Symphony of Horror',                    2, 12, 'Emily Hagins')
    ) AS t(part_public_id, sub_draft_index, movie_title, pick_position, pick_play_order, vetoer)
  LOOP
    -- STRICT raises when a query returns zero or several rows.
    SELECT dp.id, dp.draft_type INTO STRICT v_part_id, v_draft_type
    FROM drafts.draft_parts dp
    WHERE dp.public_id = r.part_public_id;

    SELECT pk.id, pk.sub_draft_id INTO STRICT v_pick_id, v_sub_id
    FROM drafts.picks pk
    JOIN drafts.movies m ON m.id = pk.movie_id
    LEFT JOIN drafts.sub_drafts sd ON sd.id = pk.sub_draft_id
    WHERE pk.draft_part_id = v_part_id
      AND pk.position = r.pick_position
      AND pk.play_order = r.pick_play_order
      AND m.movie_title = r.movie_title
      AND sd.index IS NOT DISTINCT FROM r.sub_draft_index;

    IF EXISTS (SELECT 1 FROM drafts.vetoes WHERE target_pick_id = v_pick_id) THEN
      RAISE NOTICE 'Missing vetoes: % at #% in % already has a veto, skipped.',
        r.movie_title, r.pick_position, r.part_public_id;
      CONTINUE;
    END IF;

    SELECT dpp.id INTO STRICT v_dpp_id
    FROM drafts.draft_part_participants dpp
    JOIN drafts.drafters dr ON dr.id = dpp.participant_id_value
    JOIN drafts.people pe   ON pe.id = dr.person_id
    WHERE dpp.draft_part_id = v_part_id
      AND dpp.participant_kind_value = 0
      AND pe.display_name = r.vetoer;

    INSERT INTO drafts.vetoes
      (id, acted_by_public_id, is_overridden, issued_by_participant_id, note, occurred_on,
       sequence, spent_from_fungible_pool, sub_draft_id, target_pick_id)
    VALUES
      (gen_random_uuid(), NULL, false, v_dpp_id, NULL, now(),
       1, false, v_sub_id, v_pick_id);

    -- 5 = SpeedDraft (the counter is not maintained there).
    IF v_draft_type <> 5 THEN
      UPDATE drafts.draft_part_participants
      SET vetoes_used = vetoes_used + 1
      WHERE id = v_dpp_id;
    END IF;

    v_added := v_added + 1;
    RAISE NOTICE 'Veto recorded: % at #% vetoed by %.', r.movie_title, r.pick_position, r.vetoer;
  END LOOP;

  RAISE NOTICE 'Missing vetoes: % added.', v_added;
END $$;

DO $$
DECLARE
  v_part_id uuid;
  v_sub2    uuid;
  v_sub3    uuid;
  v_pick_id uuid;
  v_n       int;
BEGIN
  SELECT dp.id INTO STRICT v_part_id
  FROM drafts.draft_parts dp WHERE dp.public_id = 'dp_7ZjG2gF2fqao7Fj';

  SELECT sd.id INTO STRICT v_sub2 FROM drafts.sub_drafts sd
  WHERE sd.draft_part_id = v_part_id AND sd.index = 2;
  SELECT sd.id INTO STRICT v_sub3 FROM drafts.sub_drafts sd
  WHERE sd.draft_part_id = v_part_id AND sd.index = 3;

  -- Already applied? Flatliners at #1 now sits in sub-draft 2.
  IF EXISTS (
    SELECT 1 FROM drafts.picks pk JOIN drafts.movies m ON m.id = pk.movie_id
    WHERE pk.draft_part_id = v_part_id AND pk.sub_draft_id = v_sub2
      AND pk.position = 1 AND m.movie_title = 'Flatliners'
  ) THEN
    RAISE NOTICE 'Speed Drafts III: Flatliners is already in sub-draft 2. Nothing to do.';
    RETURN;
  END IF;

  -- Before-state: sub-draft 2 has 8 picks (play_order 1 to 8) and no #1; sub-draft 3 has 9.
  SELECT COUNT(*) INTO v_n FROM drafts.picks WHERE draft_part_id = v_part_id AND sub_draft_id = v_sub2;
  IF v_n <> 8 THEN
    RAISE EXCEPTION 'Speed Drafts III: expected 8 picks in sub-draft 2, found %', v_n;
  END IF;
  SELECT COUNT(*) INTO v_n FROM drafts.picks
  WHERE draft_part_id = v_part_id AND sub_draft_id = v_sub2 AND position = 1;
  IF v_n <> 0 THEN
    RAISE EXCEPTION 'Speed Drafts III: sub-draft 2 already has a #1 pick (%)', v_n;
  END IF;
  SELECT COUNT(*) INTO v_n FROM drafts.picks WHERE draft_part_id = v_part_id AND sub_draft_id = v_sub3;
  IF v_n <> 9 THEN
    RAISE EXCEPTION 'Speed Drafts III: expected 9 picks in sub-draft 3, found %', v_n;
  END IF;

  SELECT pk.id INTO STRICT v_pick_id
  FROM drafts.picks pk
  JOIN drafts.movies m ON m.id = pk.movie_id
  WHERE pk.draft_part_id = v_part_id AND pk.sub_draft_id = v_sub3
    AND pk.position = 1 AND pk.play_order = 1 AND m.movie_title = 'Flatliners';

  -- Move the pick into sub-draft 2 as its last pick.
  UPDATE drafts.picks SET sub_draft_id = v_sub2, play_order = 9 WHERE id = v_pick_id;

  -- Sub-draft 3 now starts at play_order 2. Shift down by one, in two steps so the unique
  -- index on (draft_part_id, play_order, sub_draft_id) never sees a clash.
  UPDATE drafts.picks SET play_order = play_order + 1000
  WHERE draft_part_id = v_part_id AND sub_draft_id = v_sub3;
  UPDATE drafts.picks SET play_order = play_order - 1001
  WHERE draft_part_id = v_part_id AND sub_draft_id = v_sub3;

  RAISE NOTICE 'Speed Drafts III: Flatliners moved to sub-draft 2 (play_order 9); sub-draft 3 renumbered.';
END $$;

COMMIT;
