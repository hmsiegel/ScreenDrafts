-- =============================================================================
-- Administration: Add admin:view-deleted permission
-- Grants Administrator and SuperAdministrator the ability to view soft-deleted
-- records in listing and detail endpoints across all modules.
-- =============================================================================
 
INSERT INTO administration.permissions (code)
VALUES ('picks:reveal')
ON CONFLICT (code) DO NOTHING;

 
INSERT INTO administration.role_permissions (permission_code, role_name)
VALUES
    ('picks:reveal', 'Host')
ON CONFLICT (permission_code, role_name) DO NOTHING;