-- One-time backfill: a pending podcast link (Spotify, Apple) for every existing MainFeed release.
-- ResolvePodcastLinksJob picks them up. Releases older than the 7-day retry window get one lookup,
-- then NotFound if no episode matches.
-- Idempotent: the primary key (part_id, release_channel, platform) plus ON CONFLICT DO NOTHING.

START TRANSACTION;

INSERT INTO drafts.draft_release_links
  (part_id, release_channel, platform, status, next_check_at_utc, attempts, updated_on_utc)
SELECT dr.part_id, dr.release_channel, p.platform, 0, now(), 0, now()
FROM drafts.draft_releases dr
CROSS JOIN (VALUES (0), (1)) AS p(platform)
WHERE dr.release_channel = 0
ON CONFLICT DO NOTHING;

-- Expect releases x 2.
SELECT COUNT(*) AS pending_links
FROM drafts.draft_release_links
WHERE status = 0;

COMMIT;
