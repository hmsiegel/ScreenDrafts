-- Cross-schema backfill: reads administration.user_roles, writes communications.user_emails.
-- Idempotent (skips rows already true), so rerunning is harmless.
-- Syncs Patreon role holders granted before the Communications consumer existed.
UPDATE communications.user_emails ue
SET is_patreon = true
FROM administration.user_roles ur
WHERE ur.user_id = ue.user_id
  AND ur.role_name = 'Patreon'
  AND ue.is_patreon = false;