-- updates the series for the Resident Evil Super Draft to be non-canonical
DO $$
DECLARE
  v_draft_public_id  text := 'd_sfApoWxW5C8FK4m';
  v_series_public_id text := 's_dBIOgrvKYcDEoi8';
  v_draft_id  uuid;
  v_series_id uuid;
BEGIN
  SELECT id INTO v_draft_id FROM drafts.drafts WHERE public_id = v_draft_public_id;
  IF v_draft_id IS NULL THEN
    -- A fresh database (CI, new dev machine) has no drafts; skip there.
    -- Anywhere with data, a missing draft means a wrong ID: fail the migration.
    IF EXISTS (SELECT 1 FROM drafts.drafts) THEN
      RAISE EXCEPTION 'Draft % not found.', v_draft_public_id;
    END IF;
    RETURN;
  END IF;

  SELECT id INTO v_series_id FROM drafts.series WHERE public_id = v_series_public_id;
  IF v_series_id IS NULL THEN
    RAISE EXCEPTION 'Target series % not found.', v_series_public_id;
  END IF;

  -- Part A: move the draft to Special Drafts.
  UPDATE drafts.drafts                 SET series_id = v_series_id WHERE id = v_draft_id;
  UPDATE drafts.draft_parts            SET series_id = v_series_id WHERE draft_id = v_draft_id;
  UPDATE drafts.draft_channel_releases SET series_id = v_series_id WHERE draft_id = v_draft_id;

  -- Part B: reporting cleanup.
  CREATE TEMP TABLE _re_parts ON COMMIT DROP AS
  SELECT public_id FROM drafts.draft_parts WHERE draft_id = v_draft_id;

  CREATE TEMP TABLE _re_movies ON COMMIT DROP AS
  SELECT movie_public_id, MIN(picked_at) AS cut
  FROM reporting.movie_canonical_picks
  WHERE draft_part_public_id IN (SELECT public_id FROM _re_parts)
  GROUP BY movie_public_id;

  CREATE TEMP TABLE _re_drafters ON COMMIT DROP AS
  SELECT drafter_id_value, COUNT(*) AS removed, MIN(appeared_at) AS cut
  FROM reporting.drafter_canonical_appearances
  WHERE draft_part_public_id IN (SELECT public_id FROM _re_parts)
  GROUP BY drafter_id_value;

  DELETE FROM reporting.movies_honorifics_history h
  USING _re_movies m
  WHERE h.movie_public_id = m.movie_public_id AND h.achieved_at >= m.cut;

  DELETE FROM reporting.movie_canonical_picks
  WHERE draft_part_public_id IN (SELECT public_id FROM _re_parts);

  UPDATE reporting.movie_honorifics mh
  SET appearance_count     = s.cnt,
      appearance_honorific = LEAST(GREATEST(s.cnt - 1, 0), 4),
      position_honorific   =
          (CASE WHEN s.ones >= 2 THEN 1 ELSE 0 END)
        | (CASE WHEN COALESCE(s.has1 AND s.has2 AND s.has3 AND s.has4, false) THEN 2 ELSE 0 END),
      update_at_utc        = now()
  FROM (
    SELECT m.movie_public_id,
           COUNT(p.id)                                  AS cnt,
           COUNT(*) FILTER (WHERE p.board_position = 1) AS ones,
           bool_or(p.board_position = 1)                AS has1,
           bool_or(p.board_position = 2)                AS has2,
           bool_or(p.board_position = 3)                AS has3,
           bool_or(p.board_position = 4)                AS has4
    FROM _re_movies m
    LEFT JOIN reporting.movie_canonical_picks p ON p.movie_public_id = m.movie_public_id
    GROUP BY m.movie_public_id
  ) s
  WHERE mh.movie_public_id = s.movie_public_id;

  DELETE FROM reporting.drafters_honorifics_history h
  USING _re_drafters d
  WHERE h.drafter_id_value = d.drafter_id_value AND h.achieved_at >= d.cut;

  DELETE FROM reporting.drafter_canonical_appearances
  WHERE draft_part_public_id IN (SELECT public_id FROM _re_parts);

  UPDATE reporting.drafter_honorifics dh
  SET appearance_count = GREATEST(dh.appearance_count - d.removed, 0),
      honorific = CASE
        WHEN dh.appearance_count - d.removed >= 20 THEN 4
        WHEN dh.appearance_count - d.removed >= 15 THEN 3
        WHEN dh.appearance_count - d.removed >= 10 THEN 2
        WHEN dh.appearance_count - d.removed >= 5  THEN 1
        ELSE 0 END,
      update_at_utc = now()
  FROM _re_drafters d
  WHERE dh.drafter_id_value = d.drafter_id_value;
END $$;