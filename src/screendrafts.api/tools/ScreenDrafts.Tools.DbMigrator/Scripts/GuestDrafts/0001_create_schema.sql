CREATE SCHEMA IF NOT EXISTS guest_drafts;

GRANT USAGE ON SCHEMA guest_drafts TO guest_drafts_user;
GRANT CREATE ON SCHEMA guest_drafts TO guest_drafts_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA guest_drafts
    GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO guest_drafts_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA guest_drafts
    GRANT USAGE, SELECT ON SEQUENCES TO guest_drafts_user;