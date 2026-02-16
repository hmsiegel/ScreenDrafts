CREATE SCHEMA IF NOT EXISTS real_time_updates;

GRANT USAGE ON SCHEMA real_time_updates TO real_time_updates_user;
GRANT CREATE ON SCHEMA real_time_updates TO real_time_updates_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA real_time_updates
    GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO real_time_updates_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA real_time_updates
    GRANT USAGE, SELECT ON SEQUENCES TO real_time_updates_user;