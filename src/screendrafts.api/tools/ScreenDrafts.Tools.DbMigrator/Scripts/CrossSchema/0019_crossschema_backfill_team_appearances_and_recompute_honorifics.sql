-- Gives team members their appearances and recomputes drafter honorifics per draft.
-- Reporting-only reads and writes, but it needs pick_credit_facts, so it must run after
-- 0020_crossschema_backfill_record_book_facts.sql (cross-schema scripts run after the module scripts).
-- Idempotent. REVIEW ON DEV FIRST: compare reporting.drafter_honorifics before and after
-- (expected: Ryan Marker 55, Louis Peitzman 11).
--
-- 1. Appearance rows for every credited drafter in a non-excluded part (canonical_policy <> 1).
--    Solo drafters already have rows; team members do not. Proxy drafters are not credited, so they get none.
-- 2. drafter_honorifics recomputed from distinct drafts. A part counts when its policy is 0, or 2 with a
--    main-feed release (the stored flag or reporting.draft_part_releases).
-- 3. Missing tier history rows (5, 10, 15, 20 drafts) added, dated by the main-feed release date of the
--    draft that reached the threshold. Nothing is deleted. No honorific-earned events are published.
-- No explicit transaction: the DbMigrator runner wraps the script.

-- 1. Team-member appearances
INSERT INTO reporting.drafter_canonical_appearances
  (id, drafter_id_value, draft_id, draft_part_public_id, has_main_feed_release, appeared_at)
SELECT
  gen_random_uuid(),
  x.drafter_id_value,
  x.draft_id,
  x.draft_part_public_id,
  x.has_main_feed_release,
  x.appeared_at
FROM (
  SELECT DISTINCT ON (c.drafter_id_value, pf.draft_part_public_id)
    c.drafter_id_value,
    pf.draft_id,
    pf.draft_part_public_id,
    EXISTS (SELECT 1 FROM reporting.draft_part_releases r
            WHERE r.draft_part_public_id = pf.draft_part_public_id
              AND r.release_channel = 'MainFeed')                      AS has_main_feed_release,
    COALESCE((SELECT MIN(r.release_date)::timestamptz
              FROM reporting.draft_part_releases r
              WHERE r.draft_part_public_id = pf.draft_part_public_id
                AND r.release_channel = 'MainFeed'), now())             AS appeared_at
  FROM reporting.pick_credit_facts c
  JOIN reporting.pick_facts pf ON pf.id = c.pick_id
  WHERE pf.canonical_policy <> 1
) x
WHERE NOT EXISTS (
  SELECT 1 FROM reporting.drafter_canonical_appearances ca
  WHERE ca.drafter_id_value = x.drafter_id_value
    AND ca.draft_part_public_id = x.draft_part_public_id);

-- Counting set: one row per drafter per part that counts toward honorifics
DROP TABLE IF EXISTS tmp_counting_appearances;
CREATE TEMP TABLE tmp_counting_appearances AS
SELECT
  ca.drafter_id_value,
  CASE WHEN ca.draft_id = '00000000-0000-0000-0000-000000000000'::uuid
       THEN ca.draft_part_public_id
       ELSE ca.draft_id::text END                                       AS draft_key,
  COALESCE((SELECT MIN(r.release_date)::timestamptz
            FROM reporting.draft_part_releases r
            WHERE r.draft_part_public_id = ca.draft_part_public_id
              AND r.release_channel = 'MainFeed'), ca.appeared_at)      AS drafted_at
FROM reporting.drafter_canonical_appearances ca
JOIN (SELECT DISTINCT draft_part_public_id, canonical_policy FROM reporting.pick_facts) p
  ON p.draft_part_public_id = ca.draft_part_public_id
WHERE p.canonical_policy = 0
   OR (p.canonical_policy = 2
       AND (ca.has_main_feed_release
            OR EXISTS (SELECT 1 FROM reporting.draft_part_releases r
                       WHERE r.draft_part_public_id = ca.draft_part_public_id
                         AND r.release_channel = 'MainFeed')));

DROP TABLE IF EXISTS tmp_drafter_counts;
CREATE TEMP TABLE tmp_drafter_counts AS
SELECT drafter_id_value, COUNT(DISTINCT draft_key)::int AS appearance_count
FROM tmp_counting_appearances
GROUP BY drafter_id_value;

-- 2. Honorific rows
INSERT INTO reporting.drafter_honorifics
  (id, drafter_id_value, honorific, appearance_count, update_at_utc)
SELECT
  gen_random_uuid(),
  drafter_id_value,
  CASE
    WHEN appearance_count >= 20 THEN 4
    WHEN appearance_count >= 15 THEN 3
    WHEN appearance_count >= 10 THEN 2
    WHEN appearance_count >= 5  THEN 1
    ELSE 0
  END,
  appearance_count,
  now()
FROM tmp_drafter_counts
ON CONFLICT (drafter_id_value) DO UPDATE
SET honorific        = EXCLUDED.honorific,
    appearance_count = EXCLUDED.appearance_count,
    update_at_utc    = EXCLUDED.update_at_utc;

UPDATE reporting.drafter_honorifics h
SET honorific = 0, appearance_count = 0, update_at_utc = now()
WHERE h.appearance_count <> 0
  AND NOT EXISTS (SELECT 1 FROM tmp_drafter_counts c WHERE c.drafter_id_value = h.drafter_id_value);

-- 3. Missing tier history, dated by the draft that reached each threshold
INSERT INTO reporting.drafters_honorifics_history
  (id, drafter_id_value, honorific, appearance_count, achieved_at)
SELECT
  gen_random_uuid(),
  r.drafter_id_value,
  t.honorific,
  t.threshold,
  r.drafted_at
FROM (
  SELECT
    drafter_id_value,
    drafted_at,
    ROW_NUMBER() OVER (PARTITION BY drafter_id_value ORDER BY drafted_at) AS n
  FROM (
    SELECT drafter_id_value, draft_key, MIN(drafted_at) AS drafted_at
    FROM tmp_counting_appearances
    GROUP BY drafter_id_value, draft_key
  ) per_draft
) r
JOIN (VALUES (1, 5), (2, 10), (3, 15), (4, 20)) AS t(honorific, threshold)
  ON t.threshold = r.n
WHERE NOT EXISTS (
  SELECT 1 FROM reporting.drafters_honorifics_history h
  WHERE h.drafter_id_value = r.drafter_id_value
    AND h.honorific = t.honorific);

DROP TABLE tmp_counting_appearances;
DROP TABLE tmp_drafter_counts;