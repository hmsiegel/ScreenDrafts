-- Loop-friendly: define all modules here
DO
$$
DECLARE
  module TEXT;
  modules TEXT[] := ARRAY[
    'administration',
    'audit',
    'communications',
    'drafts',
    'integrations',
    'movies',
    'real-time-updates',
    'reporting',
    'users'
  ];
BEGIN
  FOREACH module IN ARRAY modules
  LOOP
    EXECUTE format('
      -- Create schema if not exists
      CREATE SCHEMA IF NOT EXISTS %I;
    ', module);

    EXECUTE format('
      -- Create login role for module
      DO $$
      BEGIN
        IF NOT EXISTS (
          SELECT 1 FROM pg_roles WHERE rolname = %L
        ) THEN
          CREATE ROLE %I LOGIN PASSWORD %L;
        END IF;
      END$$;
    ', module || '_user', module || '_user', module || '_password');

    EXECUTE format('
      -- Transfer schema ownership to module role
      ALTER SCHEMA %I OWNER TO %I;
    ', module, module || '_user');

    EXECUTE format('
      -- Grant usage and DML access to all existing objects
      GRANT USAGE ON SCHEMA %I TO %I;
      GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA %I TO %I;
      GRANT ALL ON ALL SEQUENCES IN SCHEMA %I TO %I;
    ', module, module || '_user', module, module || '_user', module, module || '_user');

    EXECUTE format('
      -- Set default privileges for future objects
      ALTER DEFAULT PRIVILEGES IN SCHEMA %I
        GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO %I;
      ALTER DEFAULT PRIVILEGES IN SCHEMA %I
        GRANT ALL ON SEQUENCES TO %I;
    ', module, module || '_user', module, module || '_user');
  END LOOP;
END
$$;
