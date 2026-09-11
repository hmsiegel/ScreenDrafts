-- 1. Add the permission
INSERT INTO administration.permissions (code)
VALUES ('guest-drafts:update'),
       ('guest-drafts:search'),
       ('guest-drafts:add-participant')
ON CONFLICT (code) DO NOTHING;

-- 2. Assign to relevant roles
INSERT INTO administration.role_permissions (permission_code, role_name)
VALUES
  ('guest-drafts:update', 'Guest'),
  ('guest-drafts:search', 'Guest'),
  ('guest-drafts:add-participant', 'Guest')
ON CONFLICT DO NOTHING;

-- 3. Backfill users.user_permissions read model
INSERT INTO users.user_permissions (user_id, permission_code)
SELECT ur.user_id, rp.permission_code
FROM administration.role_permissions rp
JOIN administration.user_roles ur ON ur.role_name = rp.role_name
WHERE rp.permission_code IN ('guest-drafts:update', 'guest-drafts:search', 'guest-drafts:add-participant')
ON CONFLICT DO NOTHING;

DELETE FROM administration.role_permissions
WHERE permission_code = 'guest-drafts:invite-participant'
  AND role_name = 'Guest';

DELETE FROM administration.permissions
WHERE code = 'guest-drafts:invite-participant';

DELETE FROM users.user_permissions
WHERE permission_code = 'guest-drafts:invite-participant';
