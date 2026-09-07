-- 1. Add the permission
INSERT INTO administration.permissions (code)
VALUES ('guest-drafts:create'),
       ('guest-drafts:read'),
       ('guest-drafts:invite-participant'),
       ('guest-drafts:set-board'),
       ('guest-drafts:set-board'),
       ('guest-drafts:assign-position'),
       ('guest-drafts:set-status'),
       ('guest-drafts:play-pick'),
       ('guest-drafts:undo-pick'),
       ('guest-drafts:apply-veto'),
       ('guest-drafts:apply-veto-override'),
       ('guest-drafts:apply-commissioner-override'),
       ('guest-drafts:undo-veto'),
       ('guest-drafts:reveal-pick')
ON CONFLICT (code) DO NOTHING;

-- 2. Assign to relevant roles
INSERT INTO administration.role_permissions (permission_code, role_name)
VALUES
  ('guest-drafts:create', 'Guest'),
  ('guest-drafts:read', 'Guest'),
  ('guest-drafts:invite-participant', 'Guest'),
  ('guest-drafts:set-board', 'Guest'),
  ('guest-drafts:assign-position', 'Guest'),
  ('guest-drafts:set-status', 'Guest'),
  ('guest-drafts:play-pick', 'Guest'),
  ('guest-drafts:undo-pick', 'Guest'),
  ('guest-drafts:apply-veto', 'Guest'),
  ('guest-drafts:apply-veto-override', 'Guest'),
  ('guest-drafts:apply-commissioner-override', 'Guest'),
  ('guest-drafts:undo-veto', 'Guest'),
  ('guest-drafts:reveal-pick', 'Guest')
ON CONFLICT DO NOTHING;

-- 3. Backfill users.user_permissions read model
INSERT INTO users.user_permissions (user_id, permission_code)
SELECT ur.user_id, rp.permission_code
FROM administration.role_permissions rp
JOIN administration.user_roles ur ON ur.role_name = rp.role_name
WHERE rp.permission_code IN ('guest-drafts:create', 'guest-drafts:read', 'guest-drafts:invite-participant', 'guest-drafts:set-board', 'guest-drafts:assign-position', 'guest-drafts:set-status', 'guest-drafts:play-pick', 'guest-drafts:undo-pick', 'guest-drafts:apply-veto', 'guest-drafts:apply-veto-override', 'guest-drafts:apply-commissioner-override', 'guest-drafts:undo-veto', 'guest-drafts:reveal-pick')
ON CONFLICT DO NOTHING;
