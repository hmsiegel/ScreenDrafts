
-- =============================================================================
-- Administration: Add categories list permissions
-- =============================================================================
 
INSERT INTO administration.permissions (code)
VALUES 
    ('drafts:list'),
    ('categories:list'),
    ('series:list')
ON CONFLICT (code) DO NOTHING;

 
INSERT INTO administration.role_permissions (permission_code, role_name)
VALUES
    ('drafts:list', 'Administrator'),
    ('drafts:list', 'SuperAdministrator'),
    ('categories:list', 'Administrator'),
    ('categories:list', 'SuperAdministrator'),
    ('series:list', 'Administrator'),
    ('series:restore', 'SuperAdministrator')
ON CONFLICT (permission_code, role_name) DO NOTHING;
