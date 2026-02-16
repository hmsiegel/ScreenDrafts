DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'integrations') THEN
        CREATE SCHEMA integrations;
    END IF;
END $EF$;
CREATE TABLE IF NOT EXISTS integrations."__EFMigrationsHistory" (
    migration_id character varying(150) NOT NULL,
    product_version character varying(32) NOT NULL,
    CONSTRAINT pk___ef_migrations_history PRIMARY KEY (migration_id)
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM integrations."__EFMigrationsHistory" WHERE "migration_id" = '20250203035420_Add_InboxAndOutbox') THEN
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'integrations') THEN
            CREATE SCHEMA integrations;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM integrations."__EFMigrationsHistory" WHERE "migration_id" = '20250203035420_Add_InboxAndOutbox') THEN
    CREATE TABLE integrations.inbox_message_consumers (
        inbox_message_id uuid NOT NULL,
        name character varying(500) NOT NULL,
        CONSTRAINT pk_inbox_message_consumers PRIMARY KEY (inbox_message_id, name)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM integrations."__EFMigrationsHistory" WHERE "migration_id" = '20250203035420_Add_InboxAndOutbox') THEN
    CREATE TABLE integrations.inbox_messages (
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
    IF NOT EXISTS(SELECT 1 FROM integrations."__EFMigrationsHistory" WHERE "migration_id" = '20250203035420_Add_InboxAndOutbox') THEN
    CREATE TABLE integrations.outbox_message_consumers (
        outbox_message_id uuid NOT NULL,
        name character varying(500) NOT NULL,
        CONSTRAINT pk_outbox_message_consumers PRIMARY KEY (outbox_message_id, name)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM integrations."__EFMigrationsHistory" WHERE "migration_id" = '20250203035420_Add_InboxAndOutbox') THEN
    CREATE TABLE integrations.outbox_messages (
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
    IF NOT EXISTS(SELECT 1 FROM integrations."__EFMigrationsHistory" WHERE "migration_id" = '20250203035420_Add_InboxAndOutbox') THEN
    INSERT INTO integrations."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20250203035420_Add_InboxAndOutbox', '10.0.3');
    END IF;
END $EF$;
COMMIT;

