-- One-time backfill: seed communications.user_emails from existing Users module data.
-- Runs under the superuser (postgres) connection which has cross-schema read rights.
-- ON CONFLICT DO UPDATE makes this safe to re-run; DbUp won't re-run it anyway.
--
-- Adjust column names below if your EF naming conventions differ:
--   users.users.email         → the Email value object column
--   users.users.first_name    → FirstName value object column
--   users.users.last_name     → LastName value object column
 
INSERT INTO communications.user_emails (user_id, email_address, full_name)
SELECT
    u.id                                            AS user_id,
    u.email                                         AS email_address,
    TRIM(CONCAT(u.first_name, ' ', u.last_name))    AS full_name
FROM users.users u
ON CONFLICT (user_id) DO UPDATE
    SET email_address = EXCLUDED.email_address,
        full_name     = EXCLUDED.full_name;
