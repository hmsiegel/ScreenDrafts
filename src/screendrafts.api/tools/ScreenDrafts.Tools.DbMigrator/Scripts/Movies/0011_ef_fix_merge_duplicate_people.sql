-- One-time data fix: merge duplicate people created by the billing-order resync.
--
-- Titles imported through the old IMDb-first path hold "legacy" people (tmdb_id = 0, IMDb id
-- set). When TMDb has no IMDb id for the same human, the resync could not match them and added
-- a second person (tmdb_id > 0, no IMDb id) on the same title. This script merges each such pair
-- into the legacy person: it takes the TMDb id and the billing order, and the duplicate goes.
--
-- A pair is merged only when a legacy and a TMDb-only person share a name (case-insensitive,
-- trimmed) on at least one title AND the pairing is one-to-one across the whole database.
-- Ambiguous pairs (homonyms, double imports) are left alone and counted in a notice.
--
-- Pinned to the data that exists: it only ever touches people with tmdb_id = 0 and an IMDb id,
-- which nothing creates any more. Idempotent: a second run finds no pairs and changes nothing.
-- Safe on an empty database. Touches the movies schema only.
--
-- NAME THIS FILE with the next number after the billing_order migration. It must run after
-- that migration, because it reads and writes movies.media_actors.billing_order.
--
-- Dev measurement (resynced copy): 4224 pairs, 8 ambiguous, 4045 actor rows dropped,
-- 316 actor rows repointed. The cap below is set well above that.

START TRANSACTION;

DO $$
DECLARE
  v_max_pairs  constant int := 6000;
  v_pairs      int;
  v_ambiguous  int;
  v_rows       int;
  v_deleted    int;
BEGIN
  CREATE TEMP TABLE credits ON COMMIT DROP AS
  SELECT c.media_id, c.person_id, p.tmdb_id, p.imdb_id, lower(btrim(p.name)) AS name_key
  FROM (
    SELECT media_id, actor_id    AS person_id FROM movies.media_actors
    UNION ALL
    SELECT media_id, director_id AS person_id FROM movies.media_directors
    UNION ALL
    SELECT media_id, writer_id   AS person_id FROM movies.media_writers
    UNION ALL
    SELECT media_id, producer_id AS person_id FROM movies.media_producers
  ) c
  JOIN movies.people p ON p.id = c.person_id;

  CREATE TEMP TABLE candidate_pairs ON COMMIT DROP AS
  SELECT DISTINCT l.person_id AS legacy_id, b.person_id AS tmdb_only_id, l.name_key
  FROM credits l
  JOIN credits b ON b.media_id = l.media_id AND b.name_key = l.name_key
  WHERE l.tmdb_id = 0
    AND l.imdb_id IS NOT NULL AND l.imdb_id <> ''
    AND b.tmdb_id > 0
    AND (b.imdb_id IS NULL OR b.imdb_id = '')
    AND l.person_id <> b.person_id;

  CREATE TEMP TABLE pairs ON COMMIT DROP AS
  SELECT cp.*
  FROM candidate_pairs cp
  WHERE (SELECT count(*) FROM candidate_pairs x WHERE x.legacy_id    = cp.legacy_id)    = 1
    AND (SELECT count(*) FROM candidate_pairs x WHERE x.tmdb_only_id = cp.tmdb_only_id) = 1;

  SELECT count(*) INTO v_pairs FROM pairs;
  SELECT count(*) - v_pairs INTO v_ambiguous FROM candidate_pairs;
  RAISE NOTICE 'duplicate people: % pairs to merge, % ambiguous pairs left alone', v_pairs, v_ambiguous;

  IF v_pairs > v_max_pairs THEN
    RAISE EXCEPTION 'merge aborted: % pairs exceeds the cap of % (expected about 4224)', v_pairs, v_max_pairs;
  END IF;

  IF v_pairs = 0 THEN
    RAISE NOTICE 'nothing to merge';
    RETURN;
  END IF;

  -- Actors: carry the billing order onto the legacy row where both rows exist on a title.
  UPDATE movies.media_actors l_row
  SET billing_order = b_row.billing_order
  FROM pairs p
  JOIN movies.media_actors b_row ON b_row.actor_id = p.tmdb_only_id
  WHERE l_row.actor_id = p.legacy_id
    AND l_row.media_id = b_row.media_id
    AND l_row.billing_order IS NULL
    AND b_row.billing_order IS NOT NULL;
  GET DIAGNOSTICS v_rows = ROW_COUNT;
  RAISE NOTICE 'actors: billing order carried onto % legacy rows', v_rows;

  -- Per role: drop the duplicate's row where the legacy person already has one on the same
  -- title (primary key is media + person), then repoint the rest.
  DELETE FROM movies.media_actors b_row USING pairs p
  WHERE b_row.actor_id = p.tmdb_only_id
    AND EXISTS (SELECT 1 FROM movies.media_actors l_row
                WHERE l_row.media_id = b_row.media_id AND l_row.actor_id = p.legacy_id);
  GET DIAGNOSTICS v_rows = ROW_COUNT;
  RAISE NOTICE 'actors: % duplicate rows dropped', v_rows;
  UPDATE movies.media_actors r SET actor_id = p.legacy_id FROM pairs p WHERE r.actor_id = p.tmdb_only_id;
  GET DIAGNOSTICS v_rows = ROW_COUNT;
  RAISE NOTICE 'actors: % rows repointed', v_rows;

  DELETE FROM movies.media_directors b_row USING pairs p
  WHERE b_row.director_id = p.tmdb_only_id
    AND EXISTS (SELECT 1 FROM movies.media_directors l_row
                WHERE l_row.media_id = b_row.media_id AND l_row.director_id = p.legacy_id);
  GET DIAGNOSTICS v_rows = ROW_COUNT;
  RAISE NOTICE 'directors: % duplicate rows dropped', v_rows;
  UPDATE movies.media_directors r SET director_id = p.legacy_id FROM pairs p WHERE r.director_id = p.tmdb_only_id;
  GET DIAGNOSTICS v_rows = ROW_COUNT;
  RAISE NOTICE 'directors: % rows repointed', v_rows;

  DELETE FROM movies.media_writers b_row USING pairs p
  WHERE b_row.writer_id = p.tmdb_only_id
    AND EXISTS (SELECT 1 FROM movies.media_writers l_row
                WHERE l_row.media_id = b_row.media_id AND l_row.writer_id = p.legacy_id);
  GET DIAGNOSTICS v_rows = ROW_COUNT;
  RAISE NOTICE 'writers: % duplicate rows dropped', v_rows;
  UPDATE movies.media_writers r SET writer_id = p.legacy_id FROM pairs p WHERE r.writer_id = p.tmdb_only_id;
  GET DIAGNOSTICS v_rows = ROW_COUNT;
  RAISE NOTICE 'writers: % rows repointed', v_rows;

  DELETE FROM movies.media_producers b_row USING pairs p
  WHERE b_row.producer_id = p.tmdb_only_id
    AND EXISTS (SELECT 1 FROM movies.media_producers l_row
                WHERE l_row.media_id = b_row.media_id AND l_row.producer_id = p.legacy_id);
  GET DIAGNOSTICS v_rows = ROW_COUNT;
  RAISE NOTICE 'producers: % duplicate rows dropped', v_rows;
  UPDATE movies.media_producers r SET producer_id = p.legacy_id FROM pairs p WHERE r.producer_id = p.tmdb_only_id;
  GET DIAGNOSTICS v_rows = ROW_COUNT;
  RAISE NOTICE 'producers: % rows repointed', v_rows;

  -- The legacy person takes the TMDb id.
  UPDATE movies.people l
  SET tmdb_id = b.tmdb_id
  FROM pairs p
  JOIN movies.people b ON b.id = p.tmdb_only_id
  WHERE l.id = p.legacy_id;
  GET DIAGNOSTICS v_rows = ROW_COUNT;
  IF v_rows <> v_pairs THEN
    RAISE EXCEPTION 'merge aborted: updated % legacy people, expected %', v_rows, v_pairs;
  END IF;

  -- A duplicate must have no credits left before it is deleted: the foreign keys cascade, so
  -- a leftover credit would be lost without a trace.
  SELECT
    (SELECT count(*) FROM movies.media_actors     r JOIN pairs p ON r.actor_id    = p.tmdb_only_id) +
    (SELECT count(*) FROM movies.media_directors  r JOIN pairs p ON r.director_id = p.tmdb_only_id) +
    (SELECT count(*) FROM movies.media_writers    r JOIN pairs p ON r.writer_id   = p.tmdb_only_id) +
    (SELECT count(*) FROM movies.media_producers  r JOIN pairs p ON r.producer_id = p.tmdb_only_id)
  INTO v_rows;
  IF v_rows > 0 THEN
    RAISE EXCEPTION 'merge aborted: % credits still point at duplicate people', v_rows;
  END IF;

  DELETE FROM movies.people b USING pairs p WHERE b.id = p.tmdb_only_id;
  GET DIAGNOSTICS v_deleted = ROW_COUNT;
  IF v_deleted <> v_pairs THEN
    RAISE EXCEPTION 'merge aborted: deleted % duplicate people, expected %', v_deleted, v_pairs;
  END IF;
  RAISE NOTICE 'merged % duplicate people into their legacy rows', v_deleted;
END $$;

COMMIT;
