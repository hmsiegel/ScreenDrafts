CREATE SCHEMA IF NOT EXISTS communications;

GRANT USAGE ON SCHEMA communications TO communications_user;
GRANT CREATE ON SCHEMA communications TO communications_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA communications
    GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO communications_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA communications
    GRANT USAGE, SELECT ON SEQUENCES TO communications_user;