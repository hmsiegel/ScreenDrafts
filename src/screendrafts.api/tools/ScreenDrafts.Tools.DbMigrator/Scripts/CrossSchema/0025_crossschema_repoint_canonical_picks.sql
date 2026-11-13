-- Repoints movie_canonical_picks rows that name a different movie record than the part's landed pick,
-- then recomputes movie honorifics. Runs after 0023. Idempotent.
-- Reporting-only reads and writes; no honorific-earned events are published, so no emails go out.
-- REVIEW ON DEV FIRST: compare reporting.movie_honorifics before and after.
--
-- Found on DEV after 0023 (a refreshed copy of PROD):
--   ep 45  and ep 246  Nashville: rows point at m_lAFyQbQxb0xDulP (tmdb 44549); the picks use
--                      m_qi7RBkOPvkeK027 (tmdb 3121, Altman's 1975 film)
--   ep 257             Hercules:  row points at m_K4mtHpQfGUmAKHl (2014); the pick is
--                      m_ON9Q9bSCuCmNLJo (Disney, 1997)
--   ep 322             Your Name.: stored appearance_count 2, one canonical row (the title was
--                      vetoed at #2, replayed at #1, and played in no other draft)
--
-- 1. A canonical row whose movie has no pick_facts row in its part is repointed to the part's landed
--    pick at the same position, but only when exactly one landed pick sits at that position and that
--    movie has no canonical row in the part yet. Anything else is left for review (verify check 14).
-- 2. Movies whose stored appearance_count differs from their canonical rows join the recompute set.
-- 3. appearance_count, appearance_honorific and position_honorific are recomputed with the rules of
--    MovieHonorificRecompute. A missing movie_honorifics row is created. History rows are written
--    only where a stored honorific changes (or a new row starts above None).
-- No explicit transaction: the DbMigrator cross-schema runner wraps every script in one.

DROP TABLE IF EXISTS tmp_affected_movies;
CREATE TEMP TABLE tmp_affected_movies (movie_public_id varchar(19) PRIMARY KEY);

-- 1. Repoint
WITH orphan AS (
  SELECT mcp.id, mcp.movie_public_id AS old_movie_public_id,
         mcp.draft_part_public_id, mcp.board_position
  FROM reporting.movie_canonical_picks mcp
  WHERE NOT EXISTS (
          SELECT 1 FROM reporting.pick_facts pf
          WHERE pf.draft_part_public_id = mcp.draft_part_public_id
            AND pf.media_public_id = mcp.movie_public_id)
    AND EXISTS (
          SELECT 1 FROM reporting.pick_facts pf
          WHERE pf.draft_part_public_id = mcp.draft_part_public_id)
),
candidate AS (
  SELECT o.id, o.old_movie_public_id, o.draft_part_public_id,
         MIN(pf.media_public_id) AS new_movie_public_id
  FROM orphan o
  JOIN reporting.pick_facts pf
    ON pf.draft_part_public_id = o.draft_part_public_id
   AND pf.position = o.board_position
   AND (NOT pf.was_vetoed OR pf.was_veto_overridden)
   AND NOT pf.was_commissioner_overridden
  GROUP BY o.id, o.old_movie_public_id, o.draft_part_public_id
  HAVING COUNT(*) = 1
),
moved AS (
  UPDATE reporting.movie_canonical_picks mcp
  SET movie_public_id = c.new_movie_public_id
  FROM candidate c
  WHERE mcp.id = c.id
    AND NOT EXISTS (
          SELECT 1 FROM reporting.movie_canonical_picks x
          WHERE x.movie_public_id = c.new_movie_public_id
            AND x.draft_part_public_id = c.draft_part_public_id)
  RETURNING c.old_movie_public_id, c.new_movie_public_id
)
INSERT INTO tmp_affected_movies
SELECT old_movie_public_id FROM moved
UNION
SELECT new_movie_public_id FROM moved
ON CONFLICT DO NOTHING;

-- 2. Stored counts that disagree with the canonical rows
INSERT INTO tmp_affected_movies
SELECT h.movie_public_id
FROM reporting.movie_honorifics h
WHERE h.appearance_count <> (SELECT COUNT(*) FROM reporting.movie_canonical_picks mcp
                             WHERE mcp.movie_public_id = h.movie_public_id)
ON CONFLICT DO NOTHING;

-- 3. Recompute
DROP TABLE IF EXISTS tmp_recomputed;
CREATE TEMP TABLE tmp_recomputed AS
SELECT
  a.movie_public_id,
  COUNT(mcp.id)::int AS appearance_count,
  CASE
    WHEN COUNT(mcp.id) >= 5 THEN 4
    WHEN COUNT(mcp.id) = 4  THEN 3
    WHEN COUNT(mcp.id) = 3  THEN 2
    WHEN COUNT(mcp.id) = 2  THEN 1
    ELSE 0
  END AS appearance_honorific,
  (CASE WHEN COUNT(*) FILTER (WHERE mcp.board_position = 1) >= 2 THEN 1 ELSE 0 END)
  | (CASE WHEN COUNT(DISTINCT mcp.board_position)
                 FILTER (WHERE mcp.board_position BETWEEN 1 AND 4) = 4 THEN 2 ELSE 0 END)
    AS position_honorific
FROM tmp_affected_movies a
LEFT JOIN reporting.movie_canonical_picks mcp ON mcp.movie_public_id = a.movie_public_id
GROUP BY a.movie_public_id;

INSERT INTO reporting.movies_honorifics_history
  (id, movie_public_id, appearance_honorific, position_honorific, appearance_count, achieved_at)
SELECT gen_random_uuid(), r.movie_public_id, r.appearance_honorific,
       r.position_honorific, r.appearance_count, now()
FROM tmp_recomputed r
JOIN reporting.movie_honorifics h ON h.movie_public_id = r.movie_public_id
WHERE h.appearance_honorific <> r.appearance_honorific
   OR h.position_honorific <> r.position_honorific;

UPDATE reporting.movie_honorifics h
SET appearance_honorific = r.appearance_honorific,
    position_honorific   = r.position_honorific,
    appearance_count     = r.appearance_count,
    update_at_utc        = now()
FROM tmp_recomputed r
WHERE h.movie_public_id = r.movie_public_id
  AND (h.appearance_honorific <> r.appearance_honorific
    OR h.position_honorific   <> r.position_honorific
    OR h.appearance_count     <> r.appearance_count);

-- Movies that gained their first canonical rows have no movie_honorifics row yet.
INSERT INTO reporting.movie_honorifics
  (id, movie_public_id, movie_title, appearance_honorific, position_honorific,
   appearance_count, update_at_utc)
SELECT gen_random_uuid(), r.movie_public_id,
       COALESCE((SELECT pf.media_title FROM reporting.pick_facts pf
                 WHERE pf.media_public_id = r.movie_public_id LIMIT 1), ''),
       r.appearance_honorific, r.position_honorific, r.appearance_count, now()
FROM tmp_recomputed r
WHERE r.appearance_count > 0
  AND NOT EXISTS (SELECT 1 FROM reporting.movie_honorifics h
                  WHERE h.movie_public_id = r.movie_public_id);

INSERT INTO reporting.movies_honorifics_history
  (id, movie_public_id, appearance_honorific, position_honorific, appearance_count, achieved_at)
SELECT gen_random_uuid(), r.movie_public_id, r.appearance_honorific,
       r.position_honorific, r.appearance_count, now()
FROM tmp_recomputed r
WHERE (r.appearance_honorific <> 0 OR r.position_honorific <> 0)
  AND NOT EXISTS (SELECT 1 FROM reporting.movies_honorifics_history hh
                  WHERE hh.movie_public_id = r.movie_public_id);

DROP TABLE IF EXISTS tmp_recomputed;
DROP TABLE IF EXISTS tmp_affected_movies;
