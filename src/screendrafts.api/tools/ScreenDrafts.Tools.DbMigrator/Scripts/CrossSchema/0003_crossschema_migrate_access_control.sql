-- =============================================================================
-- Cross-schema: Migrate access-control data to administration schema
--               and create the users.user_permissions read model.
--
-- Runs as postgres superuser after all module scripts have completed.
-- Reads from users.* (source of truth, still intact) and writes to
-- administration.* and users.user_permissions.
--
-- Safe to re-run: all inserts use ON CONFLICT DO NOTHING.
-- =============================================================================


-- -----------------------------------------------------------------------------
-- 1. Migrate permissions registry
-- -----------------------------------------------------------------------------
INSERT INTO administration.permissions (code)
SELECT code
FROM users.permissions
ON CONFLICT (code) DO NOTHING;


-- -----------------------------------------------------------------------------
-- 2. Migrate roles
-- -----------------------------------------------------------------------------
INSERT INTO administration.roles (name)
SELECT name
FROM users.roles
ON CONFLICT (name) DO NOTHING;


-- -----------------------------------------------------------------------------
-- 3. Migrate role_permissions assignments
-- -----------------------------------------------------------------------------
INSERT INTO administration.role_permissions (permission_code, role_name)
SELECT permission_code, role_name
FROM users.role_permissions
ON CONFLICT (permission_code, role_name) DO NOTHING;


-- -----------------------------------------------------------------------------
-- 4. Migrate user_roles assignments
-- Note: column order differs — users.user_roles is (role_name, user_id),
--       administration.user_roles is (user_id, role_name).
-- -----------------------------------------------------------------------------
INSERT INTO administration.user_roles (user_id, role_name)
SELECT user_id, role_name
FROM users.user_roles
ON CONFLICT (user_id, role_name) DO NOTHING;


-- -----------------------------------------------------------------------------
-- 5. Create the users.user_permissions flat read model
-- This table is the new authorization hot path — one lookup, no joins.
-- The Users module owns this table; Administration populates it via
-- integration events after this one-time seed.
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS users.user_permissions
(
    user_id         UUID         NOT NULL,
    permission_code VARCHAR(100) NOT NULL,
    CONSTRAINT pk_user_permissions PRIMARY KEY (user_id, permission_code)
);

CREATE INDEX IF NOT EXISTS ix_user_permissions_user_id
    ON users.user_permissions (user_id);


-- -----------------------------------------------------------------------------
-- 6. Seed user_permissions from the current role assignments
-- Flattens users.user_roles JOIN users.role_permissions into the read model.
-- DISTINCT handles users who hold multiple roles with overlapping permissions.
-- -----------------------------------------------------------------------------
INSERT INTO users.user_permissions (user_id, permission_code)
SELECT DISTINCT ur.user_id, rp.permission_code
FROM users.user_roles ur
JOIN users.role_permissions rp ON rp.role_name = ur.role_name
ON CONFLICT (user_id, permission_code) DO NOTHING;