-- 1. Add the permission
INSERT INTO administration.permissions (code)
VALUES ('guest-drafter:search')
ON CONFLICT (code) DO NOTHING;

-- 2. Assign to relevant roles
INSERT INTO administration.role_permissions (permission_code, role_name)
VALUES
  ('guest-drafter:search', 'Guest')
ON CONFLICT DO NOTHING;

-- 3. Backfill users.user_permissions read model
INSERT INTO users.user_permissions (user_id, permission_code)
SELECT ur.user_id, rp.permission_code
FROM administration.role_permissions rp
JOIN administration.user_roles ur ON ur.role_name = rp.role_name
WHERE rp.permission_code IN ('guest-drafter:search')
ON CONFLICT DO NOTHING;
