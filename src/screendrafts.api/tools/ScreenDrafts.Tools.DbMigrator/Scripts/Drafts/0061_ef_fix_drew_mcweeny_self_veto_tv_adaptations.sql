-- One-time data fix: Drew McWeeny's missed self-veto on TV Adaptations, part 1.
-- At #6 Drew first selected Mission: Impossible III, then vetoed it himself and played
-- Mission: Impossible - Ghost Protocol. The archive has only the replacement.
--
-- Adds: the Mission: Impossible III pick (position 6, play_order 2, played by Drew), one veto on it
-- issued by Drew (sequence 1, not overridden), and vetoes_used 0 -> 1 on Drew's participant row.
-- Picks with play_order >= 2 in that part move up by one so play order stays the order played.
-- Drew's rollover is unchanged: starting 1 + rolling in 1 + awarded 0 - used 1 still leaves 1.
--
-- Idempotent: exits with a notice when Drew already has a veto row in this part.
-- Raises, and rolls back, when the data is not in the expected before-state.
-- Drafts schema only. The Reporting read models (site_stats.vetoes_count, part stats) are not touched.

START TRANSACTION;

DO $$
DECLARE
  v_part_id   uuid;
  v_dpp_id    uuid;
  v_dpp       drafts.draft_part_participants%ROWTYPE;
  v_gp        drafts.picks%ROWTYPE;
  v_movie_id  uuid;
  v_new_pick  uuid := gen_random_uuid();
  v_count     int;
BEGIN
  SELECT COUNT(*) INTO v_count
  FROM drafts.draft_parts dp JOIN drafts.drafts d ON d.id = dp.draft_id
  WHERE d.title = 'TV Adaptations' AND dp.part_index = 1;
  IF v_count <> 1 THEN
    RAISE EXCEPTION 'Drew self-veto: expected 1 part for TV Adaptations part 1, found %', v_count;
  END IF;

  SELECT dp.id INTO v_part_id
  FROM drafts.draft_parts dp JOIN drafts.drafts d ON d.id = dp.draft_id
  WHERE d.title = 'TV Adaptations' AND dp.part_index = 1;

  SELECT COUNT(*) INTO v_count
  FROM drafts.draft_part_participants dpp
  JOIN drafts.drafters dr ON dr.id = dpp.participant_id_value
  JOIN drafts.people pe   ON pe.id = dr.person_id
  WHERE dpp.draft_part_id = v_part_id AND dpp.participant_kind_value = 0
    AND pe.first_name = 'Drew' AND pe.last_name = 'McWeeny';
  IF v_count <> 1 THEN
    RAISE EXCEPTION 'Drew self-veto: expected 1 participant row for Drew McWeeny, found %', v_count;
  END IF;

  SELECT dpp.id INTO v_dpp_id
  FROM drafts.draft_part_participants dpp
  JOIN drafts.drafters dr ON dr.id = dpp.participant_id_value
  JOIN drafts.people pe   ON pe.id = dr.person_id
  WHERE dpp.draft_part_id = v_part_id AND dpp.participant_kind_value = 0
    AND pe.first_name = 'Drew' AND pe.last_name = 'McWeeny';

  SELECT * INTO v_dpp FROM drafts.draft_part_participants WHERE id = v_dpp_id;

  -- Already applied?
  IF EXISTS (SELECT 1 FROM drafts.vetoes WHERE issued_by_participant_id = v_dpp_id) THEN
    RAISE NOTICE 'Drew self-veto: Drew already has a veto row in this part. Nothing to do.';
    RETURN;
  END IF;

  IF v_dpp.vetoes_used <> 0 THEN
    RAISE EXCEPTION 'Drew self-veto: expected vetoes_used 0, found %', v_dpp.vetoes_used;
  END IF;

  SELECT COUNT(*) INTO v_count FROM drafts.picks WHERE draft_part_id = v_part_id;
  IF v_count <> 7 THEN
    RAISE EXCEPTION 'Drew self-veto: expected 7 picks in the part, found %', v_count;
  END IF;

  SELECT COUNT(*) INTO v_count FROM drafts.movies WHERE imdb_id = 'tt0317919';
  IF v_count <> 1 THEN
    RAISE EXCEPTION 'Drew self-veto: expected 1 movie with imdb_id tt0317919, found %', v_count;
  END IF;
  SELECT id INTO v_movie_id FROM drafts.movies WHERE imdb_id = 'tt0317919';

  -- The replacement pick: position 6, play_order 2, played by Drew.
  SELECT COUNT(*) INTO v_count
  FROM drafts.picks pk
  WHERE pk.draft_part_id = v_part_id AND pk.position = 6 AND pk.play_order = 2
    AND pk.played_by_participant_id = v_dpp_id;
  IF v_count <> 1 THEN
    RAISE EXCEPTION 'Drew self-veto: expected exactly one Drew pick at position 6, play_order 2, found %', v_count;
  END IF;
  SELECT * INTO v_gp FROM drafts.picks
  WHERE draft_part_id = v_part_id AND position = 6 AND play_order = 2;

  -- Shift play_order in two steps so the unique indexes never see a clash.
  UPDATE drafts.picks SET play_order = play_order + 1000
  WHERE draft_part_id = v_part_id AND play_order >= 2;
  UPDATE drafts.picks SET play_order = play_order - 999
  WHERE draft_part_id = v_part_id AND play_order >= 1002;

  -- The vetoed pick takes the freed slot. Every other column copies from the replacement pick.
  INSERT INTO drafts.picks
    (id, acted_by_public_id, draft_part_id, movie_id, movie_version_name, play_order,
     played_by_participant_id, played_by_participant_id_value, played_by_participant_kind_value,
     position, reveal_authorized_participant_id, reveal_authorized_participant_id_value,
     reveal_authorized_participant_kind_value, revealed_at, sub_draft_id)
  VALUES
    (v_new_pick, v_gp.acted_by_public_id, v_part_id, v_movie_id, NULL, 2,
     v_gp.played_by_participant_id, v_gp.played_by_participant_id_value, v_gp.played_by_participant_kind_value,
     6, v_gp.reveal_authorized_participant_id, v_gp.reveal_authorized_participant_id_value,
     v_gp.reveal_authorized_participant_kind_value, v_gp.revealed_at, v_gp.sub_draft_id);

  INSERT INTO drafts.vetoes
    (id, acted_by_public_id, is_overridden, issued_by_participant_id, note, occurred_on,
     sequence, spent_from_fungible_pool, sub_draft_id, target_pick_id)
  VALUES
    (gen_random_uuid(), v_gp.acted_by_public_id, false, v_dpp_id, NULL, now(),
     1, false, v_gp.sub_draft_id, v_new_pick);

  UPDATE drafts.draft_part_participants SET vetoes_used = vetoes_used + 1 WHERE id = v_dpp_id;

  RAISE NOTICE 'Drew self-veto recorded: Mission: Impossible III vetoed by Drew McWeeny at #6.';
END $$;

COMMIT;
