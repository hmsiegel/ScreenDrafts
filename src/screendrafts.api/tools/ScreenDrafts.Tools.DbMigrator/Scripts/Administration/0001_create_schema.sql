CREATE SCHEMA IF NOT EXISTS administration;

GRANT USAGE ON SCHEMA administration TO administration_user;
GRANT CREATE ON SCHEMA administration TO administration_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA administration
    GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO administration_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA administration
    GRANT USAGE, SELECT ON SEQUENCES TO administration_user;