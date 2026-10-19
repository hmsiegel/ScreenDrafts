DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'communications') THEN
        CREATE SCHEMA communications;
    END IF;
END $EF$;
CREATE TABLE IF NOT EXISTS communications."__EFMigrationsHistory" (
    migration_id character varying(150) NOT NULL,
    product_version character varying(32) NOT NULL,
    CONSTRAINT pk___ef_migrations_history PRIMARY KEY (migration_id)
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM communications."__EFMigrationsHistory" WHERE "migration_id" = '20250203035002_Add_InboxAndOutbox') THEN
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'communications') THEN
            CREATE SCHEMA communications;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM communications."__EFMigrationsHistory" WHERE "migration_id" = '20250203035002_Add_InboxAndOutbox') THEN
    CREATE TABLE communications.inbox_message_consumers (
        inbox_message_id uuid NOT NULL,
        name character varying(500) NOT NULL,
        CONSTRAINT pk_inbox_message_consumers PRIMARY KEY (inbox_message_id, name)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM communications."__EFMigrationsHistory" WHERE "migration_id" = '20250203035002_Add_InboxAndOutbox') THEN
    CREATE TABLE communications.inbox_messages (
        id uuid NOT NULL,
        type text NOT NULL,
        content jsonb NOT NULL,
        occurred_on_utc timestamp with time zone NOT NULL,
        processed_on_utc timestamp with time zone,
        error text,
        CONSTRAINT pk_inbox_messages PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM communications."__EFMigrationsHistory" WHERE "migration_id" = '20250203035002_Add_InboxAndOutbox') THEN
    CREATE TABLE communications.outbox_message_consumers (
        outbox_message_id uuid NOT NULL,
        name character varying(500) NOT NULL,
        CONSTRAINT pk_outbox_message_consumers PRIMARY KEY (outbox_message_id, name)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM communications."__EFMigrationsHistory" WHERE "migration_id" = '20250203035002_Add_InboxAndOutbox') THEN
    CREATE TABLE communications.outbox_messages (
        id uuid NOT NULL,
        type text NOT NULL,
        content jsonb NOT NULL,
        occurred_on_utc timestamp with time zone NOT NULL,
        processed_on_utc timestamp with time zone,
        error text,
        CONSTRAINT pk_outbox_messages PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM communications."__EFMigrationsHistory" WHERE "migration_id" = '20250203035002_Add_InboxAndOutbox') THEN
    INSERT INTO communications."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20250203035002_Add_InboxAndOutbox', '10.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM communications."__EFMigrationsHistory" WHERE "migration_id" = '20261019210457_Add_Contact_Form') THEN
    CREATE TABLE communications.contact_messages (
        id uuid NOT NULL,
        name character varying(100) NOT NULL,
        email character varying(254) NOT NULL,
        category character varying(20) NOT NULL,
        message character varying(2000) NOT NULL,
        user_public_id character varying(50),
        created_on_utc timestamp with time zone NOT NULL,
        emailed_on_utc timestamp with time zone,
        CONSTRAINT pk_contact_messages PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM communications."__EFMigrationsHistory" WHERE "migration_id" = '20261019210457_Add_Contact_Form') THEN
    CREATE INDEX ix_contact_messages_created_on_utc ON communications.contact_messages (created_on_utc);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM communications."__EFMigrationsHistory" WHERE "migration_id" = '20261019210457_Add_Contact_Form') THEN
    INSERT INTO communications."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20261019210457_Add_Contact_Form', '10.0.11');
    END IF;
END $EF$;
COMMIT;

