-- =============================================================================
-- Patreon permission gap check — generated 2026-08-28 13:21
-- Source codes discovered in *Auth.cs across the repo.
-- =============================================================================

-- 1. Which of these codes are missing from administration.permissions entirely?
WITH found_codes(code) AS (
  VALUES
    ('drafts:read-patreon'),
    ('media:read-patreon'),
    ('patreon:search')
)
SELECT f.code AS missing_permission_code
FROM found_codes f
LEFT JOIN administration.permissions p ON p.code = f.code
WHERE p.code IS NULL;

-- 2. For codes that DO exist, which roles actually have them?
--    (Empty or missing 'Patreon'/'Administrator'/'SuperAdministrator' rows here
--    are the real gap even when the permission code itself exists.)
WITH found_codes(code) AS (
  VALUES
    ('drafts:read-patreon'),
    ('media:read-patreon'),
    ('patreon:search')
)
SELECT f.code, rp.role_name
FROM found_codes f
LEFT JOIN administration.role_permissions rp ON rp.permission_code = f.code
ORDER BY f.code, rp.role_name;

-- 3. Does the 'Patreon' role exist at all post-migration?
--    (users.roles/.role_permissions/.permissions were dropped in
--    20260420175252_Drop_Users_Access_Controls — confirm the role survived
--    the cutover to the administration schema.)
SELECT * FROM administration.roles WHERE name = 'Patreon';

-- 4. Does any user currently hold the 'Patreon' role?
SELECT * FROM administration.user_roles WHERE role_name = 'Patreon';
