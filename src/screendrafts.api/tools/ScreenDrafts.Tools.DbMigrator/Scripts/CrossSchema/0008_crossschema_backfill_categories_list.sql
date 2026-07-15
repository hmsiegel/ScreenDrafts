
INSERT INTO users.user_permissions (user_id, permission_code)
SELECT
  ur.user_id,
  rp.permission_code
FROM administration.user_roles ur
JOIN administration.role_permissions rp
  ON rp.role_name = ur.role_name
WHERE rp.permission_code IN ('drafts:list', 'categories:list', 'series:list')
  AND ur.role_name IN ('Administrator', 'SuperAdministrator')
ON CONFLICT (user_id, permission_code) DO NOTHING;
