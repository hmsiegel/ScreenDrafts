DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'guest_drafts') THEN
        CREATE SCHEMA guest_drafts;
    END IF;
END $EF$;
CREATE TABLE IF NOT EXISTS guest_drafts."__EFMigrationsHistory" (
    migration_id character varying(150) NOT NULL,
    product_version character varying(32) NOT NULL,
    CONSTRAINT pk___ef_migrations_history PRIMARY KEY (migration_id)
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260831174811_Add_Guest_Drafts') THEN
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'guest_drafts') THEN
            CREATE SCHEMA guest_drafts;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260831174811_Add_Guest_Drafts') THEN
    CREATE TABLE guest_drafts.guest_drafts (
        id uuid NOT NULL,
        public_id character varying(19) NOT NULL,
        owner_user_id uuid NOT NULL,
        title character varying(150) NOT NULL,
        guest_draft_type integer NOT NULL,
        guest_draft_status integer NOT NULL,
        share_token character varying(19),
        created_on_utc timestamp with time zone NOT NULL,
        updated_on_utc timestamp with time zone,
        CONSTRAINT pk_guest_drafts PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260831174811_Add_Guest_Drafts') THEN
    CREATE TABLE guest_drafts.inbox_message_consumers (
        inbox_message_id uuid NOT NULL,
        name character varying(500) NOT NULL,
        CONSTRAINT pk_inbox_message_consumers PRIMARY KEY (inbox_message_id, name)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260831174811_Add_Guest_Drafts') THEN
    CREATE TABLE guest_drafts.inbox_messages (
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
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260831174811_Add_Guest_Drafts') THEN
    CREATE TABLE guest_drafts.outbox_message_consumers (
        outbox_message_id uuid NOT NULL,
        name character varying(500) NOT NULL,
        CONSTRAINT pk_outbox_message_consumers PRIMARY KEY (outbox_message_id, name)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260831174811_Add_Guest_Drafts') THEN
    CREATE TABLE guest_drafts.outbox_messages (
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
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260831174811_Add_Guest_Drafts') THEN
    CREATE TABLE guest_drafts.guest_draft_participants (
        id uuid NOT NULL,
        public_id character varying(19) NOT NULL,
        guest_draft_id uuid NOT NULL,
        user_id uuid NOT NULL,
        is_owner boolean NOT NULL,
        joined_on_utc timestamp with time zone NOT NULL,
        CONSTRAINT pk_guest_draft_participants PRIMARY KEY (id),
        CONSTRAINT fk_guest_draft_participants_guest_drafts_guest_draft_id FOREIGN KEY (guest_draft_id) REFERENCES guest_drafts.guest_drafts (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260831174811_Add_Guest_Drafts') THEN
    CREATE UNIQUE INDEX ix_guest_draft_participants_guest_draft_id_user_id ON guest_drafts.guest_draft_participants (guest_draft_id, user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260831174811_Add_Guest_Drafts') THEN
    CREATE UNIQUE INDEX ix_guest_draft_participants_public_id ON guest_drafts.guest_draft_participants (public_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260831174811_Add_Guest_Drafts') THEN
    CREATE UNIQUE INDEX ix_guest_drafts_public_id ON guest_drafts.guest_drafts (public_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260831174811_Add_Guest_Drafts') THEN
    CREATE UNIQUE INDEX ix_guest_drafts_share_token ON guest_drafts.guest_drafts (share_token) WHERE share_token IS NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260831174811_Add_Guest_Drafts') THEN
    INSERT INTO guest_drafts."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20260831174811_Add_Guest_Drafts', '10.0.11');
    END IF;
END $EF$;
COMMIT;

