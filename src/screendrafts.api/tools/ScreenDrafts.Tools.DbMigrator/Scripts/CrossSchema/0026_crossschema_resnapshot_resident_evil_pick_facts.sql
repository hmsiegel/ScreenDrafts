-- Re-snapshots the series on reporting.pick_facts for the Resident Evil Super Draft. Runs after 0025. Idempotent.
-- 0023 moved the draft to Special Drafts and cleaned the canonical tables, but pick_facts keeps its own copy of
-- series_name and canonical_policy, taken when the part completed. The Record Book, the Titles tab and the stats
-- queries all filter on that copy (scoped_picks: canonical_policy = 0, or 2 with a main-feed release),
-- so they still saw the draft as Default / Always.
-- Reporting-only write; no events are published, so no emails go out.
-- After running, clear the Redis cache (record-book keys, spotlight, site stats 24h key, Titles tab).
-- No explicit transaction: the DbMigrator cross-schema runner wraps every script in one.

DO $$
DECLARE
  v_draft_public_id text := 'd_sfApoWxW5C8FK4m';
  v_draft_id        uuid;
  v_series_name     text;
  v_policy          int;
  v_updated         int;
BEGIN
  SELECT d.id, s.name, s.canonical_policy
  INTO v_draft_id, v_series_name, v_policy
  FROM drafts.drafts d
  JOIN drafts.series s ON s.id = d.series_id
  WHERE d.public_id = v_draft_public_id;

  IF v_draft_id IS NULL THEN
    -- A fresh database (CI, new dev machine) has no drafts; skip there.
    -- Anywhere with data, a missing draft means a wrong ID: fail the migration.
    IF EXISTS (SELECT 1 FROM drafts.drafts) THEN
      RAISE EXCEPTION 'Draft % not found.', v_draft_public_id;
    END IF;
    RETURN;
  END IF;

  UPDATE reporting.pick_facts
  SET series_name      = v_series_name,
      canonical_policy = v_policy
  WHERE draft_id = v_draft_id
    AND (series_name <> v_series_name OR canonical_policy <> v_policy);

  GET DIAGNOSTICS v_updated = ROW_COUNT;
  RAISE NOTICE 'pick_facts re-snapshotted for %: % row(s) now % / policy %',
    v_draft_public_id, v_updated, v_series_name, v_policy;
END $$;
