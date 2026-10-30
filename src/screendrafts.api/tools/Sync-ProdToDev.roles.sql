-- Sync-ProdToDev.roles.sql
-- Runs against the DEV database. Never run it against PROD.
-- Called by tools\Sync-ProdToDev.ps1 once per module with psql -v ON_ERROR_STOP=1 and
--   -v rol=<login role>  -v pw=<password>  -v sch=<schema>
-- Safe to run repeatedly.
--
-- Why: a PROD dump carries no roles, and --no-owner leaves every object owned by postgres.
-- The migrator and API connect as per-module roles (administration_user, ...), so each role
-- must exist with the password from your appsettings.Development.json, and must own its
-- schema's objects (a migration that ALTERs a table needs to be its owner).

-- Login role: create if missing, then set the password to match your connection string.
SELECT format('CREATE ROLE %I LOGIN PASSWORD %L', :'rol', :'pw')
WHERE NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = :'rol')
\gexec

SELECT format('ALTER ROLE %I WITH LOGIN PASSWORD %L', :'rol', :'pw')
\gexec

-- Schema and object ownership.
SELECT format('ALTER SCHEMA %I OWNER TO %I', :'sch', :'rol')
WHERE EXISTS (SELECT 1 FROM pg_namespace WHERE nspname = :'sch')
\gexec

SELECT format('ALTER TABLE %I.%I OWNER TO %I', schemaname, tablename, :'rol')
FROM pg_tables
WHERE schemaname = :'sch' AND tableowner <> :'rol'
\gexec

SELECT format('ALTER VIEW %I.%I OWNER TO %I', schemaname, viewname, :'rol')
FROM pg_views
WHERE schemaname = :'sch' AND viewowner <> :'rol'
\gexec

SELECT format('ALTER MATERIALIZED VIEW %I.%I OWNER TO %I', schemaname, matviewname, :'rol')
FROM pg_matviews
WHERE schemaname = :'sch' AND matviewowner <> :'rol'
\gexec

-- Standalone sequences only. A sequence owned by a column follows its table.
SELECT format('ALTER SEQUENCE %I.%I OWNER TO %I', s.schemaname, s.sequencename, :'rol')
FROM pg_sequences s
WHERE s.schemaname = :'sch'
  AND s.sequenceowner <> :'rol'
  AND NOT EXISTS (
    SELECT 1
    FROM pg_depend d
    WHERE d.objid = (quote_ident(s.schemaname) || '.' || quote_ident(s.sequencename))::regclass
      AND d.deptype IN ('a', 'i'))
\gexec
