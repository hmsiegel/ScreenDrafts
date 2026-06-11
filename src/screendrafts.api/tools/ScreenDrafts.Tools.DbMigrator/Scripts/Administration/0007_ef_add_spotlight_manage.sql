-- =============================================================================
-- Administration: Add attendance permissions
-- =============================================================================
 
INSERT INTO administration.permissions (code)
VALUES 
    ('spotlight:manage')
ON CONFLICT (code) DO NOTHING;

 
INSERT INTO administration.role_permissions (permission_code, role_name)
VALUES
    ('spotlight:manage', 'Administrator'),
    ('spotlight:manage', 'SuperAdministrator')
ON CONFLICT (permission_code, role_name) DO NOTHING;
