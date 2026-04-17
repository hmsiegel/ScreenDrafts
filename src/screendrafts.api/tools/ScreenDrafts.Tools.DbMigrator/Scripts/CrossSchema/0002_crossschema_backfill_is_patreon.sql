-- Backfill is_patreon on communications.user_emails from existing Users role data.
-- Runs under the postgres superuser connection (cross-schema read rights).
-- Adjust the role name string if your Patreon role is named differently.
UPDATE communications.user_emails ue
SET is_patreon = true
FROM users.user_roles ur
WHERE ur.user_id = ue.user_id
  AND ur.role_name = 'Patreon';