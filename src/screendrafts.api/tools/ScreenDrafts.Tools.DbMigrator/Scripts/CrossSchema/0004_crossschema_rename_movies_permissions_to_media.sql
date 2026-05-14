-- =============================================================================
-- Rename movies:* permissions to media:* and add media:import
-- FK is not deferrable so we must:
--   1. Insert new media:* codes into permissions
--   2. Swing role_permissions to point at the new codes
--   3. Delete the old movies:* codes from permissions
-- =============================================================================

-- -----------------------------------------------------------------------------
-- 1. Insert new permission codes alongside the old ones
-- -----------------------------------------------------------------------------
INSERT INTO administration.permissions (code)
VALUES
  ('media:search'),
  ('media:create'),
  ('media:read'),
  ('media:import')
ON CONFLICT (code) DO NOTHING;

-- -----------------------------------------------------------------------------
-- 2. Swing administration.role_permissions to the new codes
-- -----------------------------------------------------------------------------
UPDATE administration.role_permissions
SET permission_code = 'media:search'
WHERE permission_code = 'movies:search';

UPDATE administration.role_permissions
SET permission_code = 'media:create'
WHERE permission_code = 'movies:create';

UPDATE administration.role_permissions
SET permission_code = 'media:read'
WHERE permission_code = 'movies:read';

-- Assign media:import to roles
INSERT INTO administration.role_permissions (permission_code, role_name)
VALUES
  ('media:import', 'Drafter'),
  ('media:import', 'Host'),
  ('media:import', 'Administrator'),
  ('media:import', 'SuperAdministrator')
ON CONFLICT (permission_code, role_name) DO NOTHING;

-- -----------------------------------------------------------------------------
-- 3. Delete the old movies:* codes now that nothing references them
-- -----------------------------------------------------------------------------
DELETE FROM administration.permissions
WHERE code IN ('movies:search', 'movies:create', 'movies:read');

-- -----------------------------------------------------------------------------
-- 4. Swing users.user_permissions to the new codes
-- -----------------------------------------------------------------------------
UPDATE users.user_permissions
SET permission_code = 'media:search'
WHERE permission_code = 'movies:search';

UPDATE users.user_permissions
SET permission_code = 'media:create'
WHERE permission_code = 'movies:create';

UPDATE users.user_permissions
SET permission_code = 'media:read'
WHERE permission_code = 'movies:read';

-- Seed media:import for all users who hold a qualifying role
INSERT INTO users.user_permissions (user_id, permission_code)
SELECT DISTINCT ur.user_id, 'media:import'
FROM administration.user_roles ur
WHERE ur.role_name IN ('Drafter', 'Host', 'Administrator', 'SuperAdministrator')
ON CONFLICT (user_id, permission_code) DO NOTHING;