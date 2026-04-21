-- =============================================================================
-- Administration: Add admin:view-deleted permission
-- Grants Administrator and SuperAdministrator the ability to view soft-deleted
-- records in listing and detail endpoints across all modules.
-- =============================================================================
 
INSERT INTO administration.permissions (code)
VALUES ('admin:view-deleted');
 
INSERT INTO administration.role_permissions (permission_code, role_name)
VALUES
    ('admin:view-deleted', 'Administrator'),
    ('admin:view-deleted', 'SuperAdministrator');