-- =============================================================================
-- Administration: Add attendance permissions
-- =============================================================================
 
INSERT INTO administration.permissions (code)
VALUES 
    ('attendances:join'),
    ('attendances:withdraw')
ON CONFLICT (code) DO NOTHING;

 
INSERT INTO administration.role_permissions (permission_code, role_name)
VALUES
    ('attendances:join', 'Drafter'),
    ('attendances:withdraw', 'Drafter'),
    ('attendances:join', 'Host'),
    ('attendances:withdraw', 'Host'),
    ('attendances:join', 'Administrator'),
    ('attendances:withdraw', 'Administrator'),
    ('attendances:join', 'SuperAdministrator'),
    ('attendances:withdraw', 'SuperAdministrator')
ON CONFLICT (permission_code, role_name) DO NOTHING;
