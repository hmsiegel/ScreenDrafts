-- Links existing drafter_canonical_appearances rows to their draft.
-- Runs after 0008_ef_add_draft_id_to_canonical_appearances.sql, which adds draft_id with an empty-guid default.
-- Reporting-only: draft_summaries (completed parts) first, then draft_part_releases (parts still in progress).
-- Rows that match neither stay at the empty guid; the handler counts each of those as its own draft.
-- Idempotent.

UPDATE reporting.drafter_canonical_appearances ca
SET draft_id = ds.draft_id
FROM reporting.draft_summaries ds
WHERE ds.draft_part_public_id = ca.draft_part_public_id
  AND ca.draft_id = '00000000-0000-0000-0000-000000000000'::uuid;

UPDATE reporting.drafter_canonical_appearances ca
SET draft_id = r.draft_id
FROM reporting.draft_part_releases r
WHERE r.draft_part_public_id = ca.draft_part_public_id
  AND ca.draft_id = '00000000-0000-0000-0000-000000000000'::uuid;