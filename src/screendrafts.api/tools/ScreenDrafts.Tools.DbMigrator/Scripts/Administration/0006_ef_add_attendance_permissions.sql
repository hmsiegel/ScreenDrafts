-- =============================================================================
-- Administration: Add attendance permissions
-- =============================================================================
 
INSERT INTO administration.permissions (code)
VALUES 
    ('addendances:join'),
    ('attendances:withdraw')
ON CONFLICT (code) DO NOTHING;

 
INSERT INTO administration.role_permissions (permission_code, role_name)
VALUES
    ('addendances:join', 'Drafter'),
    ('attendances:withdraw', 'Drafter'),
    ('addendances:join', 'Host'),
    ('attendances:withdraw', 'Host'),
    ('addendances:join', 'Administrator'),
    ('attendances:withdraw', 'Administrator'),
    ('addendances:join', 'SuperAdministrator'),
    ('attendances:withdraw', 'SuperAdministrator')
ON CONFLICT (permission_code, role_name) DO NOTHING;
