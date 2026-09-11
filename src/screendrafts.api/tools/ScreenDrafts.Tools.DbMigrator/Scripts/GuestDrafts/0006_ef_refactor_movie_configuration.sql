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

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260901203812_Expand_Guest_Drafts') THEN
    ALTER TABLE guest_drafts.guest_draft_participants ADD awarded_fungible_tokens integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260901203812_Expand_Guest_Drafts') THEN
    ALTER TABLE guest_drafts.guest_draft_participants ADD awarded_veto_overrides integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260901203812_Expand_Guest_Drafts') THEN
    ALTER TABLE guest_drafts.guest_draft_participants ADD awarded_vetoes integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260901203812_Expand_Guest_Drafts') THEN
    ALTER TABLE guest_drafts.guest_draft_participants ADD commissioner_overrides integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260901203812_Expand_Guest_Drafts') THEN
    ALTER TABLE guest_drafts.guest_draft_participants ADD fungible_tokens integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260901203812_Expand_Guest_Drafts') THEN
    ALTER TABLE guest_drafts.guest_draft_participants ADD fungible_tokens_used integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260901203812_Expand_Guest_Drafts') THEN
    ALTER TABLE guest_drafts.guest_draft_participants ADD starting_vetoes integer NOT NULL DEFAULT 1;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260901203812_Expand_Guest_Drafts') THEN
    ALTER TABLE guest_drafts.guest_draft_participants ADD veto_overrides_used integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260901203812_Expand_Guest_Drafts') THEN
    ALTER TABLE guest_drafts.guest_draft_participants ADD vetoes_used integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260901203812_Expand_Guest_Drafts') THEN
    CREATE TABLE guest_drafts.guest_draft_game_boards (
        id uuid NOT NULL,
        guest_draft_id uuid NOT NULL,
        CONSTRAINT pk_guest_draft_game_boards PRIMARY KEY (id),
        CONSTRAINT fk_guest_draft_game_boards_guest_drafts_guest_draft_id FOREIGN KEY (guest_draft_id) REFERENCES guest_drafts.guest_drafts (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260901203812_Expand_Guest_Drafts') THEN
    CREATE TABLE guest_drafts.guest_draft_picks (
        id uuid NOT NULL,
        guest_draft_id uuid NOT NULL,
        position integer NOT NULL,
        play_order integer NOT NULL,
        movie_public_id character varying(19) NOT NULL,
        played_by_participant_id uuid NOT NULL,
        reveal_authorized_participant_id uuid,
        acted_by_public_id character varying(19),
        revealed_at timestamp with time zone,
        CONSTRAINT pk_guest_draft_picks PRIMARY KEY (id),
        CONSTRAINT fk_guest_draft_picks_guest_draft_participants_played_by_partic FOREIGN KEY (played_by_participant_id) REFERENCES guest_drafts.guest_draft_participants (id) ON DELETE RESTRICT,
        CONSTRAINT fk_guest_draft_picks_guest_draft_participants_reveal_authorize FOREIGN KEY (reveal_authorized_participant_id) REFERENCES guest_drafts.guest_draft_participants (id) ON DELETE RESTRICT,
        CONSTRAINT fk_guest_draft_picks_guest_drafts_guest_draft_id FOREIGN KEY (guest_draft_id) REFERENCES guest_drafts.guest_drafts (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260901203812_Expand_Guest_Drafts') THEN
    CREATE TABLE guest_drafts.guest_draft_positions (
        id uuid NOT NULL,
        game_board_id uuid NOT NULL,
        public_id character varying(19) NOT NULL,
        name character varying(50) NOT NULL,
        picks integer[] NOT NULL,
        has_bonus_veto boolean NOT NULL,
        has_bonus_veto_override boolean NOT NULL,
        has_bonus_fungible_token boolean NOT NULL,
        assigned_to_participant_id uuid,
        CONSTRAINT pk_guest_draft_positions PRIMARY KEY (id),
        CONSTRAINT fk_guest_draft_positions_guest_draft_game_boards_game_board_id FOREIGN KEY (game_board_id) REFERENCES guest_drafts.guest_draft_game_boards (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260901203812_Expand_Guest_Drafts') THEN
    CREATE TABLE guest_drafts.guest_draft_commissioner_overrides (
        id uuid NOT NULL,
        pick_id uuid NOT NULL,
        CONSTRAINT pk_guest_draft_commissioner_overrides PRIMARY KEY (id),
        CONSTRAINT fk_guest_draft_commissioner_overrides_guest_draft_picks_pick_id FOREIGN KEY (pick_id) REFERENCES guest_drafts.guest_draft_picks (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260901203812_Expand_Guest_Drafts') THEN
    CREATE TABLE guest_drafts.guest_draft_pick_history (
        id integer GENERATED BY DEFAULT AS IDENTITY,
        kind character varying(50) NOT NULL,
        issuer_participant_id uuid,
        note character varying(500),
        occurred_on_utc timestamp with time zone NOT NULL,
        guest_draft_pick_id uuid NOT NULL,
        CONSTRAINT pk_guest_draft_pick_history PRIMARY KEY (id),
        CONSTRAINT fk_guest_draft_pick_history_guest_draft_picks_guest_draft_pick FOREIGN KEY (guest_draft_pick_id) REFERENCES guest_drafts.guest_draft_picks (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260901203812_Expand_Guest_Drafts') THEN
    CREATE TABLE guest_drafts.guest_draft_vetoes (
        id uuid NOT NULL,
        target_pick_id uuid NOT NULL,
        sequence integer NOT NULL,
        issued_by_participant_id uuid NOT NULL,
        acted_by_public_id character varying(19),
        spent_from_fungible_pool boolean NOT NULL,
        is_overridden boolean NOT NULL,
        occurred_on timestamp with time zone NOT NULL,
        note character varying(500),
        CONSTRAINT pk_guest_draft_vetoes PRIMARY KEY (id),
        CONSTRAINT fk_guest_draft_vetoes_guest_draft_participants_issued_by_parti FOREIGN KEY (issued_by_participant_id) REFERENCES guest_drafts.guest_draft_participants (id) ON DELETE RESTRICT,
        CONSTRAINT fk_guest_draft_vetoes_guest_draft_picks_target_pick_id FOREIGN KEY (target_pick_id) REFERENCES guest_drafts.guest_draft_picks (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260901203812_Expand_Guest_Drafts') THEN
    CREATE TABLE guest_drafts.guest_draft_veto_overrides (
        id uuid NOT NULL,
        veto_id uuid NOT NULL,
        issued_by_participant_id uuid NOT NULL,
        acted_by_public_id character varying(19),
        spent_from_fungible_pool boolean NOT NULL,
        note character varying(500),
        CONSTRAINT pk_guest_draft_veto_overrides PRIMARY KEY (id),
        CONSTRAINT fk_guest_draft_veto_overrides_guest_draft_participants_issued_ FOREIGN KEY (issued_by_participant_id) REFERENCES guest_drafts.guest_draft_participants (id) ON DELETE RESTRICT,
        CONSTRAINT fk_guest_draft_veto_overrides_guest_draft_vetoes_veto_id FOREIGN KEY (veto_id) REFERENCES guest_drafts.guest_draft_vetoes (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260901203812_Expand_Guest_Drafts') THEN
    CREATE UNIQUE INDEX ix_guest_draft_commissioner_overrides_pick_id ON guest_drafts.guest_draft_commissioner_overrides (pick_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260901203812_Expand_Guest_Drafts') THEN
    CREATE UNIQUE INDEX ix_guest_draft_game_boards_guest_draft_id ON guest_drafts.guest_draft_game_boards (guest_draft_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260901203812_Expand_Guest_Drafts') THEN
    CREATE INDEX ix_guest_draft_pick_history_guest_draft_pick_id ON guest_drafts.guest_draft_pick_history (guest_draft_pick_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260901203812_Expand_Guest_Drafts') THEN
    CREATE INDEX ix_guest_draft_picks_guest_draft_id_play_order ON guest_drafts.guest_draft_picks (guest_draft_id, play_order);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260901203812_Expand_Guest_Drafts') THEN
    CREATE INDEX ix_guest_draft_picks_played_by_participant_id ON guest_drafts.guest_draft_picks (played_by_participant_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260901203812_Expand_Guest_Drafts') THEN
    CREATE INDEX ix_guest_draft_picks_reveal_authorized_participant_id ON guest_drafts.guest_draft_picks (reveal_authorized_participant_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260901203812_Expand_Guest_Drafts') THEN
    CREATE INDEX ix_guest_draft_positions_game_board_id ON guest_drafts.guest_draft_positions (game_board_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260901203812_Expand_Guest_Drafts') THEN
    CREATE UNIQUE INDEX ix_guest_draft_positions_public_id ON guest_drafts.guest_draft_positions (public_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260901203812_Expand_Guest_Drafts') THEN
    CREATE INDEX ix_guest_draft_veto_overrides_issued_by_participant_id ON guest_drafts.guest_draft_veto_overrides (issued_by_participant_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260901203812_Expand_Guest_Drafts') THEN
    CREATE UNIQUE INDEX ix_guest_draft_veto_overrides_veto_id ON guest_drafts.guest_draft_veto_overrides (veto_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260901203812_Expand_Guest_Drafts') THEN
    CREATE INDEX ix_guest_draft_vetoes_issued_by_participant_id ON guest_drafts.guest_draft_vetoes (issued_by_participant_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260901203812_Expand_Guest_Drafts') THEN
    CREATE UNIQUE INDEX ix_guest_draft_vetoes_target_pick_id_sequence ON guest_drafts.guest_draft_vetoes (target_pick_id, sequence);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260901203812_Expand_Guest_Drafts') THEN
    INSERT INTO guest_drafts."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20260901203812_Expand_Guest_Drafts', '10.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260906145553_Update_GuestDrafters') THEN
    DROP INDEX guest_drafts.ix_guest_draft_participants_guest_draft_id_user_id;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260906145553_Update_GuestDrafters') THEN
    DROP INDEX guest_drafts.ix_guest_draft_participants_public_id;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260906145553_Update_GuestDrafters') THEN
    ALTER TABLE guest_drafts.guest_draft_participants DROP COLUMN public_id;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260906145553_Update_GuestDrafters') THEN
    ALTER TABLE guest_drafts.guest_draft_participants RENAME COLUMN user_id TO participant_id_value;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260906145553_Update_GuestDrafters') THEN
    ALTER TABLE guest_drafts.guest_drafts ADD draft_date date;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260906145553_Update_GuestDrafters') THEN
    ALTER TABLE guest_drafts.guest_draft_participants ADD participant_kind_value integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260906145553_Update_GuestDrafters') THEN
    CREATE TABLE guest_drafts.guest_drafter_teams (
        id uuid NOT NULL,
        name character varying(100) NOT NULL,
        public_id character varying(19) NOT NULL,
        CONSTRAINT pk_guest_drafter_teams PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260906145553_Update_GuestDrafters') THEN
    CREATE TABLE guest_drafts.guest_drafters (
        id uuid NOT NULL,
        public_id character varying(19) NOT NULL,
        user_id uuid NOT NULL,
        first_name character varying(100) NOT NULL,
        last_name character varying(100) NOT NULL,
        CONSTRAINT pk_guest_drafters PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260906145553_Update_GuestDrafters') THEN
    CREATE TABLE guest_drafts.guest_drafter_team_members (
        drafters_id uuid NOT NULL,
        guest_drafter_team_id uuid NOT NULL,
        CONSTRAINT pk_guest_drafter_team_members PRIMARY KEY (drafters_id, guest_drafter_team_id),
        CONSTRAINT fk_guest_drafter_team_members_guest_drafter_teams_guest_drafte FOREIGN KEY (guest_drafter_team_id) REFERENCES guest_drafts.guest_drafter_teams (id) ON DELETE CASCADE,
        CONSTRAINT fk_guest_drafter_team_members_guest_drafters_drafters_id FOREIGN KEY (drafters_id) REFERENCES guest_drafts.guest_drafters (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260906145553_Update_GuestDrafters') THEN
    CREATE UNIQUE INDEX ix_guest_draft_participants_guest_draft_id_participant_id_valu ON guest_drafts.guest_draft_participants (guest_draft_id, participant_id_value, participant_kind_value);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260906145553_Update_GuestDrafters') THEN
    CREATE INDEX ix_guest_drafter_team_members_guest_drafter_team_id ON guest_drafts.guest_drafter_team_members (guest_drafter_team_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260906145553_Update_GuestDrafters') THEN
    CREATE UNIQUE INDEX ix_guest_drafter_teams_public_id ON guest_drafts.guest_drafter_teams (public_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260906145553_Update_GuestDrafters') THEN
    CREATE UNIQUE INDEX ix_guest_drafters_public_id ON guest_drafts.guest_drafters (public_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260906145553_Update_GuestDrafters') THEN
    CREATE UNIQUE INDEX ix_guest_drafters_user_id ON guest_drafts.guest_drafters (user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260906145553_Update_GuestDrafters') THEN
    INSERT INTO guest_drafts."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20260906145553_Update_GuestDrafters', '10.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260908163756_Add_GuestDraft_Movies') THEN
    ALTER TABLE guest_drafts.guest_draft_picks ADD movie_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260908163756_Add_GuestDraft_Movies') THEN
    CREATE TABLE guest_drafts.movies (
        id uuid NOT NULL,
        public_id character varying(19) NOT NULL,
        movie_title text NOT NULL,
        imdb_id text,
        tmdb_id integer,
        igdb_id integer,
        media_type integer NOT NULL,
        year character varying(10),
        tv_series_tmdb_id integer,
        season_number integer,
        episode_number integer,
        tv_series_title character varying(200),
        CONSTRAINT pk_movies PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260908163756_Add_GuestDraft_Movies') THEN
    CREATE INDEX ix_guest_draft_picks_movie_id ON guest_drafts.guest_draft_picks (movie_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260908163756_Add_GuestDraft_Movies') THEN
    CREATE UNIQUE INDEX ix_movies_public_id ON guest_drafts.movies (public_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260908163756_Add_GuestDraft_Movies') THEN
    CREATE UNIQUE INDEX ix_movies_tmdb_id ON guest_drafts.movies (tmdb_id) WHERE tmdb_id IS NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260908163756_Add_GuestDraft_Movies') THEN
    ALTER TABLE guest_drafts.guest_draft_picks ADD CONSTRAINT fk_guest_draft_picks_movies_movie_id FOREIGN KEY (movie_id) REFERENCES guest_drafts.movies (id) ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260908163756_Add_GuestDraft_Movies') THEN
    INSERT INTO guest_drafts."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20260908163756_Add_GuestDraft_Movies', '10.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260909170224_Refactor_GuestDrafts_Module') THEN
    DROP TABLE guest_drafts.guest_draft_commissioner_overrides;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260909170224_Refactor_GuestDrafts_Module') THEN
    DROP TABLE guest_drafts.guest_draft_pick_history;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260909170224_Refactor_GuestDrafts_Module') THEN
    DROP TABLE guest_drafts.guest_draft_positions;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260909170224_Refactor_GuestDrafts_Module') THEN
    DROP TABLE guest_drafts.guest_draft_veto_overrides;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260909170224_Refactor_GuestDrafts_Module') THEN
    DROP TABLE guest_drafts.guest_drafter_team_members;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260909170224_Refactor_GuestDrafts_Module') THEN
    DROP TABLE guest_drafts.guest_draft_game_boards;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260909170224_Refactor_GuestDrafts_Module') THEN
    DROP TABLE guest_drafts.guest_draft_vetoes;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260909170224_Refactor_GuestDrafts_Module') THEN
    DROP TABLE guest_drafts.guest_drafter_teams;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260909170224_Refactor_GuestDrafts_Module') THEN
    DROP TABLE guest_drafts.guest_drafters;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260909170224_Refactor_GuestDrafts_Module') THEN
    DROP TABLE guest_drafts.guest_draft_picks;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260909170224_Refactor_GuestDrafts_Module') THEN
    DROP TABLE guest_drafts.guest_draft_participants;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260909170224_Refactor_GuestDrafts_Module') THEN
    DROP TABLE guest_drafts.guest_drafts;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260909170224_Refactor_GuestDrafts_Module') THEN
    CREATE TABLE guest_drafts.drafter_teams (
        id uuid NOT NULL,
        name character varying(100) NOT NULL,
        public_id character varying(19) NOT NULL,
        CONSTRAINT pk_drafter_teams PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260909170224_Refactor_GuestDrafts_Module') THEN
    CREATE TABLE guest_drafts.drafters (
        id uuid NOT NULL,
        public_id character varying(19) NOT NULL,
        user_id uuid NOT NULL,
        first_name character varying(100) NOT NULL,
        last_name character varying(100) NOT NULL,
        CONSTRAINT pk_drafters PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260909170224_Refactor_GuestDrafts_Module') THEN
    CREATE TABLE guest_drafts.drafts (
        id uuid NOT NULL,
        public_id character varying(19) NOT NULL,
        owner_user_id uuid NOT NULL,
        title character varying(150) NOT NULL,
        guest_draft_type integer NOT NULL,
        guest_draft_status integer NOT NULL,
        share_token character varying(19),
        created_on_utc timestamp with time zone NOT NULL,
        updated_on_utc timestamp with time zone,
        draft_date date,
        CONSTRAINT pk_drafts PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260909170224_Refactor_GuestDrafts_Module') THEN
    CREATE TABLE guest_drafts.drafter_team_members (
        drafter_team_id uuid NOT NULL,
        drafters_id uuid NOT NULL,
        CONSTRAINT pk_drafter_team_members PRIMARY KEY (drafter_team_id, drafters_id),
        CONSTRAINT fk_drafter_team_members_drafter_teams_drafter_team_id FOREIGN KEY (drafter_team_id) REFERENCES guest_drafts.drafter_teams (id) ON DELETE CASCADE,
        CONSTRAINT fk_drafter_team_members_drafters_drafters_id FOREIGN KEY (drafters_id) REFERENCES guest_drafts.drafters (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260909170224_Refactor_GuestDrafts_Module') THEN
    CREATE TABLE guest_drafts.draft_participants (
        id uuid NOT NULL,
        draft_id uuid NOT NULL,
        participant_id_value uuid NOT NULL,
        participant_kind_value integer NOT NULL,
        is_owner boolean NOT NULL,
        joined_on_utc timestamp with time zone NOT NULL,
        starting_vetoes integer NOT NULL DEFAULT 1,
        awarded_vetoes integer NOT NULL DEFAULT 0,
        awarded_veto_overrides integer NOT NULL DEFAULT 0,
        commissioner_overrides integer NOT NULL DEFAULT 0,
        fungible_tokens integer NOT NULL DEFAULT 0,
        awarded_fungible_tokens integer NOT NULL DEFAULT 0,
        vetoes_used integer NOT NULL DEFAULT 0,
        veto_overrides_used integer NOT NULL DEFAULT 0,
        fungible_tokens_used integer NOT NULL DEFAULT 0,
        CONSTRAINT pk_draft_participants PRIMARY KEY (id),
        CONSTRAINT fk_draft_participants_drafts_draft_id FOREIGN KEY (draft_id) REFERENCES guest_drafts.drafts (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260909170224_Refactor_GuestDrafts_Module') THEN
    CREATE TABLE guest_drafts.game_boards (
        id uuid NOT NULL,
        draft_id uuid NOT NULL,
        CONSTRAINT pk_game_boards PRIMARY KEY (id),
        CONSTRAINT fk_game_boards_drafts_draft_id FOREIGN KEY (draft_id) REFERENCES guest_drafts.drafts (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260909170224_Refactor_GuestDrafts_Module') THEN
    CREATE TABLE guest_drafts.picks (
        id uuid NOT NULL,
        guest_draft_id uuid NOT NULL,
        position integer NOT NULL,
        play_order integer NOT NULL,
        movie_public_id character varying(19) NOT NULL,
        movie_id uuid NOT NULL,
        played_by_participant_id uuid NOT NULL,
        reveal_authorized_participant_id uuid,
        acted_by_public_id character varying(19),
        revealed_at timestamp with time zone,
        CONSTRAINT pk_picks PRIMARY KEY (id),
        CONSTRAINT fk_picks_draft_participants_played_by_participant_id FOREIGN KEY (played_by_participant_id) REFERENCES guest_drafts.draft_participants (id) ON DELETE RESTRICT,
        CONSTRAINT fk_picks_draft_participants_reveal_authorized_participant_id FOREIGN KEY (reveal_authorized_participant_id) REFERENCES guest_drafts.draft_participants (id) ON DELETE RESTRICT,
        CONSTRAINT fk_picks_drafts_guest_draft_id FOREIGN KEY (guest_draft_id) REFERENCES guest_drafts.drafts (id) ON DELETE CASCADE,
        CONSTRAINT fk_picks_movies_movie_id FOREIGN KEY (movie_id) REFERENCES guest_drafts.movies (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260909170224_Refactor_GuestDrafts_Module') THEN
    CREATE TABLE guest_drafts.draft_positions (
        id uuid NOT NULL,
        game_board_id uuid NOT NULL,
        public_id character varying(19) NOT NULL,
        name character varying(50) NOT NULL,
        picks integer[] NOT NULL,
        has_bonus_veto boolean NOT NULL,
        has_bonus_veto_override boolean NOT NULL,
        has_bonus_fungible_token boolean NOT NULL,
        assigned_to_participant_id uuid,
        CONSTRAINT pk_draft_positions PRIMARY KEY (id),
        CONSTRAINT fk_draft_positions_game_boards_game_board_id FOREIGN KEY (game_board_id) REFERENCES guest_drafts.game_boards (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260909170224_Refactor_GuestDrafts_Module') THEN
    CREATE TABLE guest_drafts.commissioner_overrides (
        id uuid NOT NULL,
        pick_id uuid NOT NULL,
        CONSTRAINT pk_commissioner_overrides PRIMARY KEY (id),
        CONSTRAINT fk_commissioner_overrides_picks_pick_id FOREIGN KEY (pick_id) REFERENCES guest_drafts.picks (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260909170224_Refactor_GuestDrafts_Module') THEN
    CREATE TABLE guest_drafts.pick_history (
        id integer GENERATED BY DEFAULT AS IDENTITY,
        kind character varying(50) NOT NULL,
        issuer_participant_id uuid,
        note character varying(500),
        occurred_on_utc timestamp with time zone NOT NULL,
        pick_id uuid NOT NULL,
        CONSTRAINT pk_pick_history PRIMARY KEY (id),
        CONSTRAINT fk_pick_history_picks_pick_id FOREIGN KEY (pick_id) REFERENCES guest_drafts.picks (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260909170224_Refactor_GuestDrafts_Module') THEN
    CREATE TABLE guest_drafts.vetoes (
        id uuid NOT NULL,
        target_pick_id uuid NOT NULL,
        sequence integer NOT NULL,
        issued_by_participant_id uuid NOT NULL,
        acted_by_public_id character varying(19),
        spent_from_fungible_pool boolean NOT NULL,
        is_overridden boolean NOT NULL,
        occurred_on timestamp with time zone NOT NULL,
        note character varying(500),
        CONSTRAINT pk_vetoes PRIMARY KEY (id),
        CONSTRAINT fk_vetoes_draft_participants_issued_by_participant_id FOREIGN KEY (issued_by_participant_id) REFERENCES guest_drafts.draft_participants (id) ON DELETE RESTRICT,
        CONSTRAINT fk_vetoes_picks_target_pick_id FOREIGN KEY (target_pick_id) REFERENCES guest_drafts.picks (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260909170224_Refactor_GuestDrafts_Module') THEN
    CREATE TABLE guest_drafts.veto_overrides (
        id uuid NOT NULL,
        veto_id uuid NOT NULL,
        issued_by_participant_id uuid NOT NULL,
        acted_by_public_id character varying(19),
        spent_from_fungible_pool boolean NOT NULL,
        note character varying(500),
        CONSTRAINT pk_veto_overrides PRIMARY KEY (id),
        CONSTRAINT fk_veto_overrides_draft_participants_issued_by_participant_id FOREIGN KEY (issued_by_participant_id) REFERENCES guest_drafts.draft_participants (id) ON DELETE RESTRICT,
        CONSTRAINT fk_veto_overrides_vetoes_veto_id FOREIGN KEY (veto_id) REFERENCES guest_drafts.vetoes (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260909170224_Refactor_GuestDrafts_Module') THEN
    CREATE UNIQUE INDEX ix_commissioner_overrides_pick_id ON guest_drafts.commissioner_overrides (pick_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260909170224_Refactor_GuestDrafts_Module') THEN
    CREATE UNIQUE INDEX ix_draft_participants_draft_id_participant_id_value_participan ON guest_drafts.draft_participants (draft_id, participant_id_value, participant_kind_value);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260909170224_Refactor_GuestDrafts_Module') THEN
    CREATE INDEX ix_draft_positions_game_board_id ON guest_drafts.draft_positions (game_board_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260909170224_Refactor_GuestDrafts_Module') THEN
    CREATE UNIQUE INDEX ix_draft_positions_public_id ON guest_drafts.draft_positions (public_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260909170224_Refactor_GuestDrafts_Module') THEN
    CREATE INDEX ix_drafter_team_members_drafters_id ON guest_drafts.drafter_team_members (drafters_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260909170224_Refactor_GuestDrafts_Module') THEN
    CREATE UNIQUE INDEX ix_drafter_teams_public_id ON guest_drafts.drafter_teams (public_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260909170224_Refactor_GuestDrafts_Module') THEN
    CREATE UNIQUE INDEX ix_drafters_public_id ON guest_drafts.drafters (public_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260909170224_Refactor_GuestDrafts_Module') THEN
    CREATE UNIQUE INDEX ix_drafters_user_id ON guest_drafts.drafters (user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260909170224_Refactor_GuestDrafts_Module') THEN
    CREATE UNIQUE INDEX ix_drafts_public_id ON guest_drafts.drafts (public_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260909170224_Refactor_GuestDrafts_Module') THEN
    CREATE UNIQUE INDEX ix_drafts_share_token ON guest_drafts.drafts (share_token) WHERE share_token IS NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260909170224_Refactor_GuestDrafts_Module') THEN
    CREATE UNIQUE INDEX ix_game_boards_draft_id ON guest_drafts.game_boards (draft_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260909170224_Refactor_GuestDrafts_Module') THEN
    CREATE INDEX ix_pick_history_pick_id ON guest_drafts.pick_history (pick_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260909170224_Refactor_GuestDrafts_Module') THEN
    CREATE INDEX ix_picks_guest_draft_id_play_order ON guest_drafts.picks (guest_draft_id, play_order);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260909170224_Refactor_GuestDrafts_Module') THEN
    CREATE INDEX ix_picks_movie_id ON guest_drafts.picks (movie_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260909170224_Refactor_GuestDrafts_Module') THEN
    CREATE INDEX ix_picks_played_by_participant_id ON guest_drafts.picks (played_by_participant_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260909170224_Refactor_GuestDrafts_Module') THEN
    CREATE INDEX ix_picks_reveal_authorized_participant_id ON guest_drafts.picks (reveal_authorized_participant_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260909170224_Refactor_GuestDrafts_Module') THEN
    CREATE INDEX ix_veto_overrides_issued_by_participant_id ON guest_drafts.veto_overrides (issued_by_participant_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260909170224_Refactor_GuestDrafts_Module') THEN
    CREATE UNIQUE INDEX ix_veto_overrides_veto_id ON guest_drafts.veto_overrides (veto_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260909170224_Refactor_GuestDrafts_Module') THEN
    CREATE INDEX ix_vetoes_issued_by_participant_id ON guest_drafts.vetoes (issued_by_participant_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260909170224_Refactor_GuestDrafts_Module') THEN
    CREATE UNIQUE INDEX ix_vetoes_target_pick_id_sequence ON guest_drafts.vetoes (target_pick_id, sequence);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260909170224_Refactor_GuestDrafts_Module') THEN
    INSERT INTO guest_drafts."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20260909170224_Refactor_GuestDrafts_Module', '10.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260910183641_Refactor_Movie_Configuration') THEN
    DROP INDEX guest_drafts.ix_movies_tmdb_id;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260910183641_Refactor_Movie_Configuration') THEN
    CREATE UNIQUE INDEX ix_movies_tmdb_id_media_type ON guest_drafts.movies (tmdb_id, media_type) WHERE tmdb_id IS NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM guest_drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260910183641_Refactor_Movie_Configuration') THEN
    INSERT INTO guest_drafts."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20260910183641_Refactor_Movie_Configuration', '10.0.11');
    END IF;
END $EF$;
COMMIT;

