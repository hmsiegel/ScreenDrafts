CREATE SCHEMA IF NOT EXISTS integrations;

GRANT USAGE ON SCHEMA integrations TO integrations_user;
GRANT CREATE ON SCHEMA integrations TO integrations_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA integrations
    GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO integrations_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA integrations
    GRANT USAGE, SELECT ON SEQUENCES TO integrations_user;