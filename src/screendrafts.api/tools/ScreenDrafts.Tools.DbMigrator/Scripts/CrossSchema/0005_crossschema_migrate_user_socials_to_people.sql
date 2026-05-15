-- Cross-schema migration: copy social handles and profile picture path
-- from users.users to drafts.people via the person_id link.
-- Runs as postgres superuser (crossschema runner).
-- Safe to run multiple times: UPDATE only touches rows where the
-- user has a linked person and the source column is non-null.

UPDATE drafts.people p
SET
    twitter_handle    = COALESCE(p.twitter_handle,    u.twitter_handle),
    instagram_handle  = COALESCE(p.instagram_handle,  u.instagram_handle),
    letterboxd_handle = COALESCE(p.letterboxd_handle, u.letterboxd_handle),
    bluesky_handle    = COALESCE(p.bluesky_handle,    u.bluesky_handle),
    profile_picture_path = COALESCE(p.profile_picture_path, u.profile_picture_path)
FROM users.users u
WHERE u.person_id = p.id
  AND (
      u.twitter_handle    IS NOT NULL
   OR u.instagram_handle  IS NOT NULL
   OR u.letterboxd_handle IS NOT NULL
   OR u.bluesky_handle    IS NOT NULL
   OR u.profile_picture_path IS NOT NULL
  );