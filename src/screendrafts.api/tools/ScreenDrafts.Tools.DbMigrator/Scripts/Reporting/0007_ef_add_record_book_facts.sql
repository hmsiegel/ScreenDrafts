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
    VALUES ('20250203035919_Add_InboxAndOutbox', '10.0.11');
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
    VALUES ('20260327153908_Add_Honorifics', '10.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM reporting."__EFMigrationsHistory" WHERE "migration_id" = '20260505164921_Add_Home_Page_Read_Models') THEN
    CREATE TABLE reporting.draft_part_releases (
        id uuid NOT NULL,
        draft_id uuid NOT NULL,
        draft_part_public_id text NOT NULL,
        release_channel text NOT NULL,
        release_date date NOT NULL,
        CONSTRAINT pk_draft_part_releases PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM reporting."__EFMigrationsHistory" WHERE "migration_id" = '20260505164921_Add_Home_Page_Read_Models') THEN
    CREATE TABLE reporting.draft_spotlights (
        id uuid NOT NULL,
        draft_public_id text NOT NULL,
        spotlight_description text NOT NULL,
        spotify_url text,
        is_active boolean NOT NULL,
        is_pinned boolean NOT NULL,
        activated_at_utc timestamp with time zone,
        created_at_utc timestamp with time zone NOT NULL,
        CONSTRAINT pk_draft_spotlights PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM reporting."__EFMigrationsHistory" WHERE "migration_id" = '20260505164921_Add_Home_Page_Read_Models') THEN
    CREATE TABLE reporting.draft_summaries (
        id uuid NOT NULL,
        draft_id uuid NOT NULL,
        draft_public_id text NOT NULL,
        draft_part_public_id text NOT NULL,
        title text NOT NULL,
        draft_type text NOT NULL,
        part_index integer NOT NULL,
        total_parts integer NOT NULL,
        is_patreon boolean NOT NULL,
        episode_number integer,
        is_complete boolean NOT NULL,
        completed_at_utc timestamp with time zone,
        created_at_utc timestamp with time zone NOT NULL,
        CONSTRAINT pk_draft_summaries PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM reporting."__EFMigrationsHistory" WHERE "migration_id" = '20260505164921_Add_Home_Page_Read_Models') THEN
    CREATE TABLE reporting.site_stats (
        id uuid NOT NULL,
        vetoes_count integer NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_site_stats PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM reporting."__EFMigrationsHistory" WHERE "migration_id" = '20260505164921_Add_Home_Page_Read_Models') THEN
    CREATE INDEX ix_draft_part_releases_draft_id ON reporting.draft_part_releases (draft_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM reporting."__EFMigrationsHistory" WHERE "migration_id" = '20260505164921_Add_Home_Page_Read_Models') THEN
    CREATE UNIQUE INDEX ux_draft_part_releases_part_public_id_channel ON reporting.draft_part_releases (draft_part_public_id, release_channel);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM reporting."__EFMigrationsHistory" WHERE "migration_id" = '20260505164921_Add_Home_Page_Read_Models') THEN
    CREATE UNIQUE INDEX uix_draft_spotlights_active
      ON reporting.draft_spotlights (is_active)
      WHERE is_active = true;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM reporting."__EFMigrationsHistory" WHERE "migration_id" = '20260505164921_Add_Home_Page_Read_Models') THEN
    CREATE INDEX ix_draft_summaries_draft_id ON reporting.draft_summaries (draft_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM reporting."__EFMigrationsHistory" WHERE "migration_id" = '20260505164921_Add_Home_Page_Read_Models') THEN
    CREATE INDEX ix_draft_summaries_draft_public_id ON reporting.draft_summaries (draft_public_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM reporting."__EFMigrationsHistory" WHERE "migration_id" = '20260505164921_Add_Home_Page_Read_Models') THEN
    CREATE UNIQUE INDEX ux_draft_summaries_draft_id_part_public_id ON reporting.draft_summaries (draft_id, draft_part_public_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM reporting."__EFMigrationsHistory" WHERE "migration_id" = '20260505164921_Add_Home_Page_Read_Models') THEN
    INSERT INTO reporting."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20260505164921_Add_Home_Page_Read_Models', '10.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM reporting."__EFMigrationsHistory" WHERE "migration_id" = '20260508135657_Add_Total_Picks_And_Fix_DraftType_Names') THEN
    ALTER TABLE reporting.draft_summaries ADD total_picks integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM reporting."__EFMigrationsHistory" WHERE "migration_id" = '20260508135657_Add_Total_Picks_And_Fix_DraftType_Names') THEN
    UPDATE reporting.draft_summaries
    SET draft_type = CASE draft_type
        WHEN '0' THEN 'Standard'
        WHEN '1' THEN 'MiniMega'
        WHEN '2' THEN 'Mega'
        WHEN '3' THEN 'Super'
        WHEN '4' THEN 'MiniSuper'
        WHEN '5' THEN 'SpeedDraft'
        ELSE draft_type
    END
    WHERE draft_type IN ('0', '1', '2', '3', '4', '5');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM reporting."__EFMigrationsHistory" WHERE "migration_id" = '20260508135657_Add_Total_Picks_And_Fix_DraftType_Names') THEN
    INSERT INTO reporting."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20260508135657_Add_Total_Picks_And_Fix_DraftType_Names', '10.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM reporting."__EFMigrationsHistory" WHERE "migration_id" = '20260610193514_Add_Spotlight_Public_Id') THEN
    ALTER TABLE reporting.draft_spotlights ADD public_id text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM reporting."__EFMigrationsHistory" WHERE "migration_id" = '20260610193514_Add_Spotlight_Public_Id') THEN
    INSERT INTO reporting."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20260610193514_Add_Spotlight_Public_Id', '10.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM reporting."__EFMigrationsHistory" WHERE "migration_id" = '20261004131944_Add_Record_Book_Facts') THEN
    CREATE TABLE reporting.pick_credit_facts (
        id uuid NOT NULL,
        pick_id uuid NOT NULL,
        draft_id uuid NOT NULL,
        draft_part_public_id text NOT NULL,
        drafter_id_value uuid NOT NULL,
        drafter_public_id text NOT NULL,
        drafter_name text NOT NULL,
        recorded_at_utc timestamp with time zone NOT NULL,
        CONSTRAINT pk_pick_credit_facts PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM reporting."__EFMigrationsHistory" WHERE "migration_id" = '20261004131944_Add_Record_Book_Facts') THEN
    CREATE TABLE reporting.pick_facts (
        id uuid NOT NULL,
        draft_id uuid NOT NULL,
        draft_public_id text NOT NULL,
        draft_part_public_id text NOT NULL,
        part_index integer NOT NULL,
        draft_title text NOT NULL,
        draft_type text NOT NULL,
        series_name text NOT NULL,
        canonical_policy integer NOT NULL,
        sub_draft_index integer,
        position integer NOT NULL,
        media_public_id text NOT NULL,
        media_title text NOT NULL,
        played_by_kind integer NOT NULL,
        played_by_id_value uuid NOT NULL,
        played_by_public_id text,
        played_by_name text NOT NULL,
        veto_count integer NOT NULL,
        was_vetoed boolean NOT NULL,
        was_veto_overridden boolean NOT NULL,
        was_commissioner_overridden boolean NOT NULL,
        recorded_at_utc timestamp with time zone NOT NULL,
        CONSTRAINT pk_pick_facts PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM reporting."__EFMigrationsHistory" WHERE "migration_id" = '20261004131944_Add_Record_Book_Facts') THEN
    CREATE TABLE reporting.veto_facts (
        id uuid NOT NULL,
        pick_id uuid NOT NULL,
        draft_id uuid NOT NULL,
        draft_part_public_id text NOT NULL,
        sequence integer NOT NULL,
        issued_by_kind integer NOT NULL,
        issued_by_id_value uuid NOT NULL,
        issued_by_public_id text,
        issued_by_name text NOT NULL,
        is_overridden boolean NOT NULL,
        overridden_by_kind integer,
        overridden_by_id_value uuid,
        overridden_by_public_id text,
        overridden_by_name text,
        is_self_veto boolean NOT NULL,
        recorded_at_utc timestamp with time zone NOT NULL,
        CONSTRAINT pk_veto_facts PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM reporting."__EFMigrationsHistory" WHERE "migration_id" = '20261004131944_Add_Record_Book_Facts') THEN
    CREATE INDEX ix_pick_credit_facts_draft_id ON reporting.pick_credit_facts (draft_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM reporting."__EFMigrationsHistory" WHERE "migration_id" = '20261004131944_Add_Record_Book_Facts') THEN
    CREATE INDEX ix_pick_credit_facts_draft_part_public_id ON reporting.pick_credit_facts (draft_part_public_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM reporting."__EFMigrationsHistory" WHERE "migration_id" = '20261004131944_Add_Record_Book_Facts') THEN
    CREATE INDEX ix_pick_credit_facts_drafter_id_value ON reporting.pick_credit_facts (drafter_id_value);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM reporting."__EFMigrationsHistory" WHERE "migration_id" = '20261004131944_Add_Record_Book_Facts') THEN
    CREATE UNIQUE INDEX ux_pick_credit_facts_pick_id_drafter_id_value ON reporting.pick_credit_facts (pick_id, drafter_id_value);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM reporting."__EFMigrationsHistory" WHERE "migration_id" = '20261004131944_Add_Record_Book_Facts') THEN
    CREATE INDEX ix_pick_facts_draft_id ON reporting.pick_facts (draft_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM reporting."__EFMigrationsHistory" WHERE "migration_id" = '20261004131944_Add_Record_Book_Facts') THEN
    CREATE INDEX ix_pick_facts_draft_part_public_id ON reporting.pick_facts (draft_part_public_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM reporting."__EFMigrationsHistory" WHERE "migration_id" = '20261004131944_Add_Record_Book_Facts') THEN
    CREATE INDEX ix_pick_facts_media_public_id ON reporting.pick_facts (media_public_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM reporting."__EFMigrationsHistory" WHERE "migration_id" = '20261004131944_Add_Record_Book_Facts') THEN
    CREATE INDEX ix_pick_facts_played_by ON reporting.pick_facts (played_by_kind, played_by_id_value);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM reporting."__EFMigrationsHistory" WHERE "migration_id" = '20261004131944_Add_Record_Book_Facts') THEN
    CREATE INDEX ix_veto_facts_draft_id ON reporting.veto_facts (draft_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM reporting."__EFMigrationsHistory" WHERE "migration_id" = '20261004131944_Add_Record_Book_Facts') THEN
    CREATE INDEX ix_veto_facts_draft_part_public_id ON reporting.veto_facts (draft_part_public_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM reporting."__EFMigrationsHistory" WHERE "migration_id" = '20261004131944_Add_Record_Book_Facts') THEN
    CREATE INDEX ix_veto_facts_issued_by ON reporting.veto_facts (issued_by_kind, issued_by_id_value);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM reporting."__EFMigrationsHistory" WHERE "migration_id" = '20261004131944_Add_Record_Book_Facts') THEN
    CREATE INDEX ix_veto_facts_pick_id ON reporting.veto_facts (pick_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM reporting."__EFMigrationsHistory" WHERE "migration_id" = '20261004131944_Add_Record_Book_Facts') THEN
    INSERT INTO reporting."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20261004131944_Add_Record_Book_Facts', '10.0.11');
    END IF;
END $EF$;
COMMIT;

