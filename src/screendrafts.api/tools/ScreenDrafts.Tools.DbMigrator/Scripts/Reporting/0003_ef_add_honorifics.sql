DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'reporting') THEN
        CREATE SCHEMA reporting;
    END IF;
END $EF$;
CREATE TABLE IF NOT EXISTS reporting."__EFMigrationsHistory" (
    migration_id character varying(150) NOT NULL,
    product_version character varying(32) NOT NULL,
    CONSTRAINT pk___ef_migrations_history PRIMARY KEY (migration_id)
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM reporting."__EFMigrationsHistory" WHERE "migration_id" = '20250203035919_Add_InboxAndOutbox') THEN
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'reporting') THEN
            CREATE SCHEMA reporting;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM reporting."__EFMigrationsHistory" WHERE "migration_id" = '20250203035919_Add_InboxAndOutbox') THEN
    CREATE TABLE reporting.inbox_message_consumers (
        inbox_message_id uuid NOT NULL,
        name character varying(500) NOT NULL,
        CONSTRAINT pk_inbox_message_consumers PRIMARY KEY (inbox_message_id, name)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM reporting."__EFMigrationsHistory" WHERE "migration_id" = '20250203035919_Add_InboxAndOutbox') THEN
    CREATE TABLE reporting.inbox_messages (
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
    IF NOT EXISTS(SELECT 1 FROM reporting."__EFMigrationsHistory" WHERE "migration_id" = '20250203035919_Add_InboxAndOutbox') THEN
    CREATE TABLE reporting.outbox_message_consumers (
        outbox_message_id uuid NOT NULL,
        name character varying(500) NOT NULL,
        CONSTRAINT pk_outbox_message_consumers PRIMARY KEY (outbox_message_id, name)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM reporting."__EFMigrationsHistory" WHERE "migration_id" = '20250203035919_Add_InboxAndOutbox') THEN
    CREATE TABLE reporting.outbox_messages (
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
    IF NOT EXISTS(SELECT 1 FROM reporting."__EFMigrationsHistory" WHERE "migration_id" = '20250203035919_Add_InboxAndOutbox') THEN
    INSERT INTO reporting."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20250203035919_Add_InboxAndOutbox', '10.0.3');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM reporting."__EFMigrationsHistory" WHERE "migration_id" = '20260327153908_Add_Honorifics') THEN
    CREATE TABLE reporting.drafter_canonical_appearances (
        id uuid NOT NULL,
        drafter_id_value uuid NOT NULL,
        draft_part_public_id character varying(19) NOT NULL,
        has_main_feed_release boolean NOT NULL,
        appeared_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_drafter_canonical_appearances PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM reporting."__EFMigrationsHistory" WHERE "migration_id" = '20260327153908_Add_Honorifics') THEN
    CREATE TABLE reporting.drafter_honorifics (
        id uuid NOT NULL,
        drafter_id_value uuid NOT NULL,
        honorific integer NOT NULL,
        appearance_count integer NOT NULL,
        update_at_utc timestamp with time zone NOT NULL,
        CONSTRAINT pk_drafter_honorifics PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM reporting."__EFMigrationsHistory" WHERE "migration_id" = '20260327153908_Add_Honorifics') THEN
    CREATE TABLE reporting.drafters_honorifics_history (
        id uuid NOT NULL,
        drafter_id_value uuid NOT NULL,
        honorific integer NOT NULL,
        appearance_count integer NOT NULL,
        achieved_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_drafters_honorifics_history PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM reporting."__EFMigrationsHistory" WHERE "migration_id" = '20260327153908_Add_Honorifics') THEN
    CREATE TABLE reporting.movie_canonical_picks (
        id uuid NOT NULL,
        movie_public_id character varying(19) NOT NULL,
        draft_part_public_id character varying(19) NOT NULL,
        board_position integer NOT NULL,
        picked_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_movie_canonical_picks PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM reporting."__EFMigrationsHistory" WHERE "migration_id" = '20260327153908_Add_Honorifics') THEN
    CREATE TABLE reporting.movie_honorifics (
        id uuid NOT NULL,
        movie_public_id character varying(19) NOT NULL,
        movie_title text NOT NULL,
        appearance_honorific integer NOT NULL,
        position_honorific integer NOT NULL,
        appearance_count integer NOT NULL,
        update_at_utc timestamp with time zone NOT NULL,
        CONSTRAINT pk_movie_honorifics PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM reporting."__EFMigrationsHistory" WHERE "migration_id" = '20260327153908_Add_Honorifics') THEN
    CREATE TABLE reporting.movies_honorifics_history (
        id uuid NOT NULL,
        movie_public_id character varying(19) NOT NULL,
        appearance_honorific integer NOT NULL,
        position_honorific integer NOT NULL,
        appearance_count integer NOT NULL,
        achieved_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_movies_honorifics_history PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM reporting."__EFMigrationsHistory" WHERE "migration_id" = '20260327153908_Add_Honorifics') THEN
    CREATE INDEX ix_drafter_canonical_appearances_drafter_id_value ON reporting.drafter_canonical_appearances (drafter_id_value);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM reporting."__EFMigrationsHistory" WHERE "migration_id" = '20260327153908_Add_Honorifics') THEN
    CREATE UNIQUE INDEX ux_drafter_canonical_appearances_drafter_id_part_id ON reporting.drafter_canonical_appearances (drafter_id_value, draft_part_public_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM reporting."__EFMigrationsHistory" WHERE "migration_id" = '20260327153908_Add_Honorifics') THEN
    CREATE UNIQUE INDEX ux_drafter_honorifics_drafter_id_value ON reporting.drafter_honorifics (drafter_id_value);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM reporting."__EFMigrationsHistory" WHERE "migration_id" = '20260327153908_Add_Honorifics') THEN
    CREATE INDEX ix_drafter_honorifics_history_drafter_id_achieved_at ON reporting.drafters_honorifics_history (drafter_id_value, achieved_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM reporting."__EFMigrationsHistory" WHERE "migration_id" = '20260327153908_Add_Honorifics') THEN
    CREATE INDEX ix_movie_canonical_picks_movie_public_id ON reporting.movie_canonical_picks (movie_public_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM reporting."__EFMigrationsHistory" WHERE "migration_id" = '20260327153908_Add_Honorifics') THEN
    CREATE UNIQUE INDEX ux_movie_canonical_picks_movie_public_id_part_public_id ON reporting.movie_canonical_picks (movie_public_id, draft_part_public_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM reporting."__EFMigrationsHistory" WHERE "migration_id" = '20260327153908_Add_Honorifics') THEN
    CREATE UNIQUE INDEX ux_movie_honorifics_movie_public_id ON reporting.movie_honorifics (movie_public_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM reporting."__EFMigrationsHistory" WHERE "migration_id" = '20260327153908_Add_Honorifics') THEN
    CREATE INDEX ix_movie_honorifics_history_movie_public_id_achieved_at ON reporting.movies_honorifics_history (movie_public_id, achieved_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM reporting."__EFMigrationsHistory" WHERE "migration_id" = '20260327153908_Add_Honorifics') THEN
    INSERT INTO reporting."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20260327153908_Add_Honorifics', '10.0.3');
    END IF;
END $EF$;
COMMIT;

