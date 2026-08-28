-- =============================================================================
-- Fix Patreon permission gaps for media appearances (#4) and backfill the
-- drafts:read-patreon Patreon-role grant that was never made.
--
-- NOTE: rename to the correct next NNNN_crossschema_... sequence number
-- before running — verify against the latest file in
-- infrastructure/migrations (or wherever the sequence lives) first.
-- =============================================================================

-- 1. media:read-patreon did not exist in administration.permissions at all.
INSERT INTO administration.permissions (code)
VALUES ('media:read-patreon')
ON CONFLICT (code) DO NOTHING;

-- 2. Grant to Administrator, SuperAdministrator, and Patreon.
INSERT INTO administration.role_permissions (permission_code, role_name)
VALUES
  ('media:read-patreon', 'Administrator'),
  ('media:read-patreon', 'SuperAdministrator'),
  ('media:read-patreon', 'Patreon')
ON CONFLICT (permission_code, role_name) DO NOTHING;

-- 3. Backfill users.user_permissions for every user currently holding one of
--    those three roles.
INSERT INTO users.user_permissions (user_id, permission_code)
SELECT ur.user_id, rp.permission_code
FROM administration.role_permissions rp
JOIN administration.user_roles ur ON ur.role_name = rp.role_name
WHERE rp.permission_code = 'media:read-patreon'
ON CONFLICT DO NOTHING;

-- =============================================================================
-- Separate bug: drafts:read-patreon exists but was NEVER granted to Patreon —
-- only Administrator/SuperAdministrator. Real Patreon-tier members have never
-- been able to see Patreon-gated content on the /drafts archive.
-- =============================================================================

-- 4. Grant the missing role.
INSERT INTO administration.role_permissions (permission_code, role_name)
VALUES ('drafts:read-patreon', 'Patreon')
ON CONFLICT (permission_code, role_name) DO NOTHING;

-- 5. Backfill users.user_permissions for Patreon-role users specifically
--    (Administrator/SuperAdministrator already have it from a prior backfill).
INSERT INTO users.user_permissions (user_id, permission_code)
SELECT ur.user_id, rp.permission_code
FROM administration.role_permissions rp
JOIN administration.user_roles ur ON ur.role_name = rp.role_name
WHERE rp.permission_code = 'drafts:read-patreon'
  AND ur.role_name = 'Patreon'
ON CONFLICT DO NOTHING;

-- =============================================================================
-- Verification — run after the above
-- =============================================================================
SELECT permission_code, role_name
FROM administration.role_permissions
WHERE permission_code IN ('media:read-patreon', 'drafts:read-patreon')
ORDER BY permission_code, role_name;
