-- infrastructure/test/scrub.sql
-- Runs on TEST right after the restore, before any app container starts.
-- Replaces real email addresses. Stops at the first error (ON_ERROR_STOP).
--
-- Column names are inferred from the EF naming convention. If an UPDATE fails
-- on a column name, fix it here and rerun restore-test.sh.

\echo '--- Columns that might hold personal data (review this list) ---'
SELECT table_schema || '.' || table_name || '.' || column_name AS candidate
FROM information_schema.columns
WHERE table_schema NOT IN ('pg_catalog', 'information_schema')
  AND column_name ~* '(email|phone|ip_address|ip|token|secret|address)'
ORDER BY 1;

\echo '--- Scrubbing ---'
BEGIN;

UPDATE users.users
SET email = 'user-' || public_id || '@example.invalid';

UPDATE communications.user_emails
SET email_address = 'user-' || user_id || '@example.invalid';

COMMIT;

\echo '--- Done. Add an UPDATE above for any candidate column that holds personal data. ---'
