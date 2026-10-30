-- Sync-ProdToDev.post-restore.sql
-- Runs against the DEV database right after a PROD restore. Never run it against PROD.
-- Called by tools\Sync-ProdToDev.ps1 with psql -v ON_ERROR_STOP=1.

-- ============================================================================
-- 1) Re-grant module roles.
--    The dump is restored with --no-owner --no-privileges, so every table is owned
--    by postgres and the per-module roles the API connects as (drafts_user, ...)
--    have no rights. Role name = schema name with '-' -> '_' plus '_user'.
--    Roles are cluster-level, so they survive the database drop; only grants are redone.
-- ============================================================================
DO $$
DECLARE
  sch  TEXT;
  rol  TEXT;
BEGIN
  FOR sch IN
    SELECT schema_name
    FROM information_schema.schemata
    WHERE schema_name NOT IN ('public', 'information_schema')
      AND schema_name NOT LIKE 'pg\_%'
  LOOP
    rol := replace(sch, '-', '_') || '_user';
    IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = rol) THEN
      EXECUTE format('GRANT USAGE ON SCHEMA %I TO %I', sch, rol);
      EXECUTE format('GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA %I TO %I', sch, rol);
      EXECUTE format('GRANT ALL ON ALL SEQUENCES IN SCHEMA %I TO %I', sch, rol);
      EXECUTE format('ALTER DEFAULT PRIVILEGES IN SCHEMA %I GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO %I', sch, rol);
      EXECUTE format('ALTER DEFAULT PRIVILEGES IN SCHEMA %I GRANT ALL ON SEQUENCES TO %I', sch, rol);
      RAISE NOTICE 'granted % on schema %', rol, sch;
    ELSE
      RAISE NOTICE 'no role % for schema % (skipped)', rol, sch;
    END IF;
  END LOOP;
END
$$;

-- ============================================================================
-- 2) Neutralize in-flight messages.
--    PROD's outbox/inbox may hold rows that were not yet processed when the dump ran.
--    Left alone, DEV's background jobs would replay them (emails to Papercut, Zoom,
--    TMDb calls, ...). Mark every unprocessed row as processed.
--    Discovered dynamically so new modules are covered without editing this file.
-- ============================================================================
DO $$
DECLARE
  r RECORD;
  n BIGINT;
BEGIN
  FOR r IN
    SELECT t.table_schema, t.table_name
    FROM information_schema.tables t
    JOIN information_schema.columns c
      ON c.table_schema = t.table_schema
     AND c.table_name = t.table_name
     AND c.column_name = 'processed_on_utc'
    WHERE t.table_name IN ('outbox_messages', 'inbox_messages')
  LOOP
    EXECUTE format(
      'UPDATE %I.%I SET processed_on_utc = now() WHERE processed_on_utc IS NULL',
      r.table_schema, r.table_name);
    GET DIAGNOSTICS n = ROW_COUNT;
    RAISE NOTICE '%.%: marked % unprocessed row(s) as processed', r.table_schema, r.table_name, n;
  END LOOP;
END
$$;
