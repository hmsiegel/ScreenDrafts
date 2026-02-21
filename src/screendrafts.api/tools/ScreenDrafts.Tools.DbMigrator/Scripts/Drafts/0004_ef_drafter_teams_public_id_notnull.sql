DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'drafts') THEN
        CREATE SCHEMA drafts;
    END IF;
END $EF$;
CREATE TABLE IF NOT EXISTS drafts."__EFMigrationsHistory" (
    migration_id character varying(150) NOT NULL,
    product_version character varying(32) NOT NULL,
    CONSTRAINT pk___ef_migrations_history PRIMARY KEY (migration_id)
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'drafts') THEN
            CREATE SCHEMA drafts;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE TABLE drafts.campaigns (
        id uuid NOT NULL,
        public_id character varying(19) NOT NULL,
        slug character varying(100) NOT NULL,
        name character varying(200) NOT NULL,
        created_at_utc timestamp with time zone NOT NULL,
        updated_at_utc timestamp with time zone,
        is_deleted boolean NOT NULL,
        CONSTRAINT pk_campaigns PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE TABLE drafts.categories (
        id uuid NOT NULL,
        public_id character varying(19) NOT NULL,
        name character varying(100) NOT NULL,
        description character varying(500),
        created_on_utc timestamp with time zone NOT NULL,
        modified_on_utc timestamp with time zone,
        is_deleted boolean NOT NULL,
        CONSTRAINT pk_categories PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE TABLE drafts.drafter_teams (
        id uuid NOT NULL,
        name character varying(100) NOT NULL,
        number_of_drafters integer NOT NULL,
        CONSTRAINT pk_drafter_teams PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE TABLE drafts.inbox_message_consumers (
        inbox_message_id uuid NOT NULL,
        name character varying(500) NOT NULL,
        CONSTRAINT pk_inbox_message_consumers PRIMARY KEY (inbox_message_id, name)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE TABLE drafts.inbox_messages (
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
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE TABLE drafts.movies (
        id uuid NOT NULL,
        movie_title text NOT NULL,
        imdb_id text NOT NULL,
        CONSTRAINT pk_movies PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE TABLE drafts.outbox_message_consumers (
        outbox_message_id uuid NOT NULL,
        name character varying(500) NOT NULL,
        CONSTRAINT pk_outbox_message_consumers PRIMARY KEY (outbox_message_id, name)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE TABLE drafts.outbox_messages (
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
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE TABLE drafts.people (
        id uuid NOT NULL,
        user_id uuid,
        public_id text NOT NULL,
        first_name text NOT NULL,
        last_name text NOT NULL,
        display_name text,
        CONSTRAINT pk_people PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE TABLE drafts.series (
        id uuid NOT NULL,
        name character varying(200) NOT NULL,
        public_id character varying(19) NOT NULL,
        kind integer NOT NULL,
        canonical_policy integer NOT NULL,
        continuity_scope integer NOT NULL,
        continuity_date_rule integer NOT NULL,
        default_draft_type integer,
        created_at_utc timestamp with time zone NOT NULL,
        updated_at_utc timestamp with time zone,
        allowed_draft_types integer NOT NULL,
        CONSTRAINT pk_series PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE TABLE drafts.movie_versions (
        id uuid NOT NULL,
        name character varying(100) NOT NULL,
        movie_id uuid NOT NULL,
        CONSTRAINT pk_movie_versions PRIMARY KEY (id),
        CONSTRAINT fk_movie_versions_movies_movie_id FOREIGN KEY (movie_id) REFERENCES drafts.movies (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE TABLE drafts.drafters (
        id uuid NOT NULL,
        public_id character varying(19) NOT NULL,
        person_id uuid NOT NULL,
        is_retired boolean NOT NULL,
        retired_at_utc timestamp with time zone,
        CONSTRAINT pk_drafters PRIMARY KEY (id),
        CONSTRAINT fk_drafters_people_person_id FOREIGN KEY (person_id) REFERENCES drafts.people (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE TABLE drafts.hosts (
        id uuid NOT NULL,
        public_id character varying(19) NOT NULL,
        person_id uuid NOT NULL,
        CONSTRAINT pk_hosts PRIMARY KEY (id),
        CONSTRAINT fk_hosts_people_person_id FOREIGN KEY (person_id) REFERENCES drafts.people (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE TABLE drafts.drafts (
        id uuid NOT NULL,
        public_id character varying(19) NOT NULL,
        title character varying(255) NOT NULL,
        description text,
        created_at_utc timestamp with time zone NOT NULL,
        updated_at_utc timestamp with time zone,
        series_id uuid NOT NULL,
        draft_type integer NOT NULL,
        draft_status integer NOT NULL,
        campaign_id uuid,
        CONSTRAINT pk_drafts PRIMARY KEY (id),
        CONSTRAINT fk_drafts_campaigns_campaign_id FOREIGN KEY (campaign_id) REFERENCES drafts.campaigns (id) ON DELETE SET NULL,
        CONSTRAINT fk_drafts_series_series_id FOREIGN KEY (series_id) REFERENCES drafts.series (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE TABLE drafts.drafter_team_drafter (
        drafter_id uuid NOT NULL,
        drafter_team_id uuid NOT NULL,
        CONSTRAINT pk_drafter_team_drafter PRIMARY KEY (drafter_id, drafter_team_id),
        CONSTRAINT fk_drafter_team_drafter_drafter_teams_drafter_team_id FOREIGN KEY (drafter_team_id) REFERENCES drafts.drafter_teams (id) ON DELETE CASCADE,
        CONSTRAINT fk_drafter_team_drafter_drafters_drafter_id FOREIGN KEY (drafter_id) REFERENCES drafts.drafters (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE TABLE drafts.draft_categories (
        draft_id uuid NOT NULL,
        category_id uuid NOT NULL,
        CONSTRAINT pk_draft_categories PRIMARY KEY (draft_id, category_id),
        CONSTRAINT fk_draft_categories_categories_category_id FOREIGN KEY (category_id) REFERENCES drafts.categories (id) ON DELETE RESTRICT,
        CONSTRAINT fk_draft_categories_drafts_draft_id FOREIGN KEY (draft_id) REFERENCES drafts.drafts (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE TABLE drafts.draft_parts (
        id uuid NOT NULL,
        draft_id uuid NOT NULL,
        part_index integer NOT NULL,
        status integer NOT NULL,
        scheduled_for_utc timestamp with time zone,
        series_id uuid NOT NULL,
        draft_type integer NOT NULL,
        min_position integer NOT NULL,
        max_position integer NOT NULL,
        created_at_utc timestamp with time zone NOT NULL,
        updated_at_utc timestamp with time zone,
        movie_version_policy_type integer NOT NULL,
        CONSTRAINT pk_draft_parts PRIMARY KEY (id),
        CONSTRAINT fk_draft_parts_drafts_draft_id FOREIGN KEY (draft_id) REFERENCES drafts.drafts (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE TABLE drafts.draft_hosts (
        draft_part_id uuid NOT NULL,
        host_id uuid NOT NULL,
        role integer NOT NULL,
        CONSTRAINT pk_draft_hosts PRIMARY KEY (draft_part_id, host_id),
        CONSTRAINT fk_draft_hosts_draft_parts_draft_part_id FOREIGN KEY (draft_part_id) REFERENCES drafts.draft_parts (id) ON DELETE CASCADE,
        CONSTRAINT fk_draft_hosts_hosts_host_id FOREIGN KEY (host_id) REFERENCES drafts.hosts (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE TABLE drafts.draft_part_participants (
        id uuid NOT NULL,
        draft_part_id uuid NOT NULL,
        participant_id_value uuid NOT NULL,
        participant_kind_value integer NOT NULL,
        starting_vetoes integer NOT NULL,
        rollover_veto integer NOT NULL,
        rollover_veto_override integer NOT NULL,
        trivia_vetoes integer NOT NULL,
        trivia_veto_overrides integer NOT NULL,
        commissioner_overrides integer NOT NULL,
        vetoes_used integer NOT NULL,
        veto_overrides_used integer NOT NULL,
        CONSTRAINT pk_draft_part_participants PRIMARY KEY (id),
        CONSTRAINT fk_draft_part_participants_draft_parts_draft_part_id FOREIGN KEY (draft_part_id) REFERENCES drafts.draft_parts (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE TABLE drafts.draft_part_required_movie_versions (
        id uuid NOT NULL,
        movie_id uuid NOT NULL,
        version_name character varying(100) NOT NULL,
        draft_part_id uuid NOT NULL,
        CONSTRAINT pk_draft_part_required_movie_versions PRIMARY KEY (id),
        CONSTRAINT fk_draft_part_required_movie_versions_draft_parts_draft_part_id FOREIGN KEY (draft_part_id) REFERENCES drafts.draft_parts (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE TABLE drafts.draft_releases (
        part_id uuid NOT NULL,
        release_channel integer NOT NULL,
        release_date date NOT NULL,
        episode_number integer,
        created_on_utc timestamp with time zone NOT NULL,
        CONSTRAINT pk_draft_releases PRIMARY KEY (part_id, release_channel, release_date),
        CONSTRAINT fk_draft_releases_draft_parts_part_id FOREIGN KEY (part_id) REFERENCES drafts.draft_parts (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE TABLE drafts.game_boards (
        id uuid NOT NULL,
        draft_part_id uuid NOT NULL,
        CONSTRAINT pk_game_boards PRIMARY KEY (id),
        CONSTRAINT fk_game_boards_draft_parts_draft_part_id FOREIGN KEY (draft_part_id) REFERENCES drafts.draft_parts (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE TABLE drafts.trivia_results (
        id uuid NOT NULL,
        draft_part_id uuid NOT NULL,
        position integer NOT NULL,
        questions_won integer NOT NULL,
        participant_kind integer NOT NULL,
        participant_id uuid NOT NULL,
        CONSTRAINT pk_trivia_results PRIMARY KEY (id),
        CONSTRAINT fk_trivia_results_draft_parts_draft_part_id FOREIGN KEY (draft_part_id) REFERENCES drafts.draft_parts (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE TABLE drafts.draft_positions (
        id uuid NOT NULL,
        game_board_id uuid NOT NULL,
        name character varying(50) NOT NULL,
        picks text NOT NULL,
        has_bonus_veto boolean NOT NULL,
        has_bonus_veto_override boolean NOT NULL,
        assigned_to_id uuid,
        assigned_to_kind integer,
        CONSTRAINT pk_draft_positions PRIMARY KEY (id),
        CONSTRAINT fk_draft_positions_game_boards_game_board_id FOREIGN KEY (game_board_id) REFERENCES drafts.game_boards (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE TABLE drafts.commissioner_overrides (
        id uuid NOT NULL,
        pick_id uuid NOT NULL,
        CONSTRAINT pk_commissioner_overrides PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE TABLE drafts.picks (
        id uuid NOT NULL,
        position integer NOT NULL,
        play_order integer NOT NULL,
        movie_id uuid NOT NULL,
        movie_version_name character varying(100),
        draft_part_id uuid NOT NULL,
        played_by_participant_id uuid NOT NULL,
        commissioner_override_id uuid NOT NULL,
        CONSTRAINT pk_picks PRIMARY KEY (id),
        CONSTRAINT fk_picks_commissioner_overrides_commissioner_override_id FOREIGN KEY (commissioner_override_id) REFERENCES drafts.commissioner_overrides (id) ON DELETE CASCADE,
        CONSTRAINT fk_picks_draft_part_participants_played_by_participant_id FOREIGN KEY (played_by_participant_id) REFERENCES drafts.draft_part_participants (id) ON DELETE RESTRICT,
        CONSTRAINT fk_picks_draft_parts_draft_part_id FOREIGN KEY (draft_part_id) REFERENCES drafts.draft_parts (id) ON DELETE CASCADE,
        CONSTRAINT fk_picks_movies_movie_id FOREIGN KEY (movie_id) REFERENCES drafts.movies (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE TABLE drafts.vetoes (
        id uuid NOT NULL,
        target_pick_id uuid NOT NULL,
        issued_by_participant_id uuid NOT NULL,
        is_overridden boolean NOT NULL,
        occurred_on timestamp with time zone NOT NULL,
        note character varying(1000),
        CONSTRAINT pk_vetoes PRIMARY KEY (id),
        CONSTRAINT fk_vetoes_draft_part_participants_issued_by_participant_id FOREIGN KEY (issued_by_participant_id) REFERENCES drafts.draft_part_participants (id) ON DELETE RESTRICT,
        CONSTRAINT fk_vetoes_picks_target_pick_id FOREIGN KEY (target_pick_id) REFERENCES drafts.picks (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE TABLE drafts.veto_overrides (
        id uuid NOT NULL,
        veto_id uuid NOT NULL,
        issued_by_participant_id uuid NOT NULL,
        CONSTRAINT pk_veto_overrides PRIMARY KEY (id),
        CONSTRAINT fk_veto_overrides_draft_part_participants_issued_by_participan FOREIGN KEY (issued_by_participant_id) REFERENCES drafts.draft_part_participants (id) ON DELETE RESTRICT,
        CONSTRAINT fk_veto_overrides_vetoes_veto_id FOREIGN KEY (veto_id) REFERENCES drafts.vetoes (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE INDEX ix_campaigns_id ON drafts.campaigns (id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE UNIQUE INDEX ix_campaigns_public_id ON drafts.campaigns (public_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE UNIQUE INDEX ix_campaigns_slug ON drafts.campaigns (slug);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE UNIQUE INDEX ix_categories_public_id ON drafts.categories (public_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE UNIQUE INDEX ix_commissioner_overrides_pick_id ON drafts.commissioner_overrides (pick_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE INDEX ix_draft_categories_category_id ON drafts.draft_categories (category_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE INDEX ix_draft_hosts_host_id ON drafts.draft_hosts (host_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE UNIQUE INDEX ux_draft_hosts_one_primary_per_draft_part ON drafts.draft_hosts (draft_part_id) WHERE role = 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE UNIQUE INDEX ux_draft_part_participants_unique ON drafts.draft_part_participants (draft_part_id, participant_id_value, participant_kind_value);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE UNIQUE INDEX ix_draft_part_required_movie_versions_draft_part_id_movie_id ON drafts.draft_part_required_movie_versions (draft_part_id, movie_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE UNIQUE INDEX ix_draft_parts_draft_id_part_index ON drafts.draft_parts (draft_id, part_index);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE INDEX ix_draft_positions_game_board_id ON drafts.draft_positions (game_board_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE INDEX ix_draft_releases_release_channel_release_date ON drafts.draft_releases (release_channel, release_date);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE UNIQUE INDEX ux_draft_releases_mainfeed_episode_number ON drafts.draft_releases (episode_number) WHERE release_channel = 0 AND episode_number is not null;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE INDEX ix_drafter_team_drafter_drafter_team_id ON drafts.drafter_team_drafter (drafter_team_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE UNIQUE INDEX ix_drafters_person_id ON drafts.drafters (person_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE UNIQUE INDEX ix_drafters_public_id ON drafts.drafters (public_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE INDEX ix_drafts_campaign_id ON drafts.drafts (campaign_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE UNIQUE INDEX ix_drafts_public_id ON drafts.drafts (public_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE INDEX ix_drafts_series_id ON drafts.drafts (series_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE UNIQUE INDEX ix_game_boards_draft_part_id ON drafts.game_boards (draft_part_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE UNIQUE INDEX ix_hosts_person_id ON drafts.hosts (person_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE UNIQUE INDEX ix_hosts_public_id ON drafts.hosts (public_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE UNIQUE INDEX ix_movie_versions_movie_id_name ON drafts.movie_versions (movie_id, name);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE UNIQUE INDEX ix_people_public_id ON drafts.people (public_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE UNIQUE INDEX ix_picks_commissioner_override_id ON drafts.picks (commissioner_override_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE UNIQUE INDEX ix_picks_draft_part_id_play_order ON drafts.picks (draft_part_id, play_order);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE INDEX ix_picks_draft_part_id_position ON drafts.picks (draft_part_id, position);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE INDEX ix_picks_movie_id ON drafts.picks (movie_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE INDEX ix_picks_played_by_participant_id_play_order ON drafts.picks (played_by_participant_id, play_order);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE UNIQUE INDEX ix_series_public_id ON drafts.series (public_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE INDEX ix_trivia_results_draft_part_id ON drafts.trivia_results (draft_part_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE INDEX ix_veto_overrides_issued_by_participant_id ON drafts.veto_overrides (issued_by_participant_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE UNIQUE INDEX ix_veto_overrides_veto_id ON drafts.veto_overrides (veto_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE UNIQUE INDEX ix_veto_overrides_veto_id_issued_by_participant_id ON drafts.veto_overrides (veto_id, issued_by_participant_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE UNIQUE INDEX ix_vetoes_issued_by_participant_id_target_pick_id ON drafts.vetoes (issued_by_participant_id, target_pick_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    CREATE UNIQUE INDEX ix_vetoes_target_pick_id ON drafts.vetoes (target_pick_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    ALTER TABLE drafts.commissioner_overrides ADD CONSTRAINT fk_commissioner_overrides_picks_pick_id FOREIGN KEY (pick_id) REFERENCES drafts.picks (id) ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260209151650_Initial_Drafts') THEN
    INSERT INTO drafts."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20260209151650_Initial_Drafts', '10.0.3');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260211122547_Add_PlayedByParticipantIdentity_ToPicks') THEN
    ALTER TABLE drafts.picks ADD played_by_participant_id_value uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260211122547_Add_PlayedByParticipantIdentity_ToPicks') THEN
    ALTER TABLE drafts.picks ADD played_by_participant_kind_value integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260211122547_Add_PlayedByParticipantIdentity_ToPicks') THEN
    CREATE INDEX ix_picks_draft_part_id_played_by_participant_id_value_played_b ON drafts.picks (draft_part_id, played_by_participant_id_value, played_by_participant_kind_value);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260211122547_Add_PlayedByParticipantIdentity_ToPicks') THEN
    INSERT INTO drafts."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20260211122547_Add_PlayedByParticipantIdentity_ToPicks', '10.0.3');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260212121413_RemoveCommissionerOverrideIdFromPick') THEN
    ALTER TABLE drafts.picks DROP CONSTRAINT fk_picks_commissioner_overrides_commissioner_override_id;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260212121413_RemoveCommissionerOverrideIdFromPick') THEN
    DROP INDEX drafts.ix_veto_overrides_veto_id_issued_by_participant_id;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260212121413_RemoveCommissionerOverrideIdFromPick') THEN
    DROP INDEX drafts.ix_picks_commissioner_override_id;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260212121413_RemoveCommissionerOverrideIdFromPick') THEN
    ALTER TABLE drafts.picks DROP COLUMN commissioner_override_id;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260212121413_RemoveCommissionerOverrideIdFromPick') THEN
    ALTER TABLE drafts.draft_parts ALTER COLUMN min_position DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260212121413_RemoveCommissionerOverrideIdFromPick') THEN
    ALTER TABLE drafts.draft_parts ALTER COLUMN max_position DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260212121413_RemoveCommissionerOverrideIdFromPick') THEN
    INSERT INTO drafts."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20260212121413_RemoveCommissionerOverrideIdFromPick', '10.0.3');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260213185309_Update_EpisodeNumber_Draft_DraftPart') THEN
    ALTER TABLE drafts.draft_releases DROP CONSTRAINT pk_draft_releases;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260213185309_Update_EpisodeNumber_Draft_DraftPart') THEN
    DROP INDEX drafts.ux_draft_releases_mainfeed_episode_number;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260213185309_Update_EpisodeNumber_Draft_DraftPart') THEN
    ALTER TABLE drafts.draft_releases ADD draft_id uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260213185309_Update_EpisodeNumber_Draft_DraftPart') THEN

              UPDATE drafts.draft_releases dr
              SET draft_id = dp.draft_id
              FROM drafts.draft_parts dp
              WHERE dp.id = dr.part_id AND dr.draft_id IS NULL;
              
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260213185309_Update_EpisodeNumber_Draft_DraftPart') THEN

              ALTER TABLE drafts.draft_releases
              ALTER COLUMN draft_id SET NOT NULL;
              
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260213185309_Update_EpisodeNumber_Draft_DraftPart') THEN
    ALTER TABLE drafts.draft_releases ADD CONSTRAINT pk_draft_releases PRIMARY KEY (part_id, release_channel);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260213185309_Update_EpisodeNumber_Draft_DraftPart') THEN
    CREATE INDEX ix_draft_releases_draft_id ON drafts.draft_releases (draft_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260213185309_Update_EpisodeNumber_Draft_DraftPart') THEN
    CREATE UNIQUE INDEX ux_draft_releases_channel_episode_number ON drafts.draft_releases (release_channel, episode_number, draft_id) WHERE episode_number is not null;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260213185309_Update_EpisodeNumber_Draft_DraftPart') THEN
    ALTER TABLE drafts.draft_releases ADD CONSTRAINT fk_draft_releases_drafts_draft_id FOREIGN KEY (draft_id) REFERENCES drafts.drafts (id) ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260213185309_Update_EpisodeNumber_Draft_DraftPart') THEN
    INSERT INTO drafts."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20260213185309_Update_EpisodeNumber_Draft_DraftPart', '10.0.3');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260214150842_Add_DraftChannelRelease') THEN
    ALTER TABLE drafts.draft_releases DROP CONSTRAINT fk_draft_releases_drafts_draft_id;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260214150842_Add_DraftChannelRelease') THEN
    DROP INDEX drafts.ix_draft_releases_draft_id;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260214150842_Add_DraftChannelRelease') THEN
    DROP INDEX drafts.ux_draft_releases_channel_episode_number;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260214150842_Add_DraftChannelRelease') THEN
    ALTER TABLE drafts.draft_releases DROP COLUMN draft_id;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260214150842_Add_DraftChannelRelease') THEN
    ALTER TABLE drafts.draft_releases DROP COLUMN episode_number;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260214150842_Add_DraftChannelRelease') THEN
    CREATE TABLE drafts.draft_channel_releases (
        draft_id uuid NOT NULL,
        release_channel integer NOT NULL,
        episode_number integer,
        created_on_utc timestamp with time zone NOT NULL,
        CONSTRAINT pk_draft_channel_releases PRIMARY KEY (draft_id, release_channel),
        CONSTRAINT fk_draft_channel_releases_drafts_draft_id FOREIGN KEY (draft_id) REFERENCES drafts.drafts (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260214150842_Add_DraftChannelRelease') THEN
    CREATE UNIQUE INDEX ux_draft_channel_releases_channel_episode_number ON drafts.draft_channel_releases (release_channel, episode_number) WHERE episode_number is not null;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260214150842_Add_DraftChannelRelease') THEN
    INSERT INTO drafts."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20260214150842_Add_DraftChannelRelease', '10.0.3');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260215211450_Add_Series_To_DraftChannelRelease') THEN
    DROP INDEX drafts.ux_draft_channel_releases_channel_episode_number;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260215211450_Add_Series_To_DraftChannelRelease') THEN
    ALTER TABLE drafts.draft_channel_releases ADD series_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260215211450_Add_Series_To_DraftChannelRelease') THEN
    CREATE INDEX ix_draft_channel_releases_series_id ON drafts.draft_channel_releases (series_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260215211450_Add_Series_To_DraftChannelRelease') THEN
    CREATE UNIQUE INDEX ux_draft_channel_releases_channel_series_episode_number ON drafts.draft_channel_releases (release_channel, episode_number, series_id) WHERE episode_number is not null;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260215211450_Add_Series_To_DraftChannelRelease') THEN
    ALTER TABLE drafts.draft_channel_releases ADD CONSTRAINT fk_draft_channel_releases_series_series_id FOREIGN KEY (series_id) REFERENCES drafts.series (id) ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260215211450_Add_Series_To_DraftChannelRelease') THEN
    INSERT INTO drafts."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20260215211450_Add_Series_To_DraftChannelRelease', '10.0.3');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260220213402_Add_PublicIds') THEN
    ALTER TABLE drafts.drafter_teams ADD public_id character varying(100) DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260220213402_Add_PublicIds') THEN
    INSERT INTO drafts."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20260220213402_Add_PublicIds', '10.0.3');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260221163304_Update_DrafterTeams') THEN
    CREATE UNIQUE INDEX ix_drafter_teams_public_id ON drafts.drafter_teams (public_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260221163304_Update_DrafterTeams') THEN
    ALTER TABLE drafts.drafter_teams ALTER COLUMN public_id DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM drafts."__EFMigrationsHistory" WHERE "migration_id" = '20260221163304_Update_DrafterTeams') THEN
    INSERT INTO drafts."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20260221163304_Update_DrafterTeams', '10.0.3');
    END IF;
END $EF$;
COMMIT;

