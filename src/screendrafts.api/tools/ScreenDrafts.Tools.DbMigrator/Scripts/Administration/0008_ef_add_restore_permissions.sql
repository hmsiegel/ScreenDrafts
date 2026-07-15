-- =============================================================================
-- Administration: Add restore permissions
-- =============================================================================
 
INSERT INTO administration.permissions (code)
VALUES 
    ('drafts:restore'),
    ('categories:restore'),
    ('series:restore')
ON CONFLICT (code) DO NOTHING;

 
INSERT INTO administration.role_permissions (permission_code, role_name)
VALUES
    ('drafts:restore', 'Administrator'),
    ('drafts:restore', 'SuperAdministrator'),
    ('categories:restore', 'Administrator'),
    ('categories:restore', 'SuperAdministrator'),
    ('series:restore', 'Administrator'),
    ('series:restore', 'SuperAdministrator')
ON CONFLICT (permission_code, role_name) DO NOTHING;
