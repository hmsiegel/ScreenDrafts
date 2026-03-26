DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'movies') THEN
        CREATE SCHEMA movies;
    END IF;
END $EF$;
CREATE TABLE IF NOT EXISTS movies."__EFMigrationsHistory" (
    migration_id character varying(150) NOT NULL,
    product_version character varying(32) NOT NULL,
    CONSTRAINT pk___ef_migrations_history PRIMARY KEY (migration_id)
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM movies."__EFMigrationsHistory" WHERE "migration_id" = '20250326005904_Add_Initial') THEN
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'movies') THEN
            CREATE SCHEMA movies;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM movies."__EFMigrationsHistory" WHERE "migration_id" = '20250326005904_Add_Initial') THEN
    CREATE TABLE movies.genres (
        id uuid NOT NULL,
        name text NOT NULL,
        CONSTRAINT pk_genres PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM movies."__EFMigrationsHistory" WHERE "migration_id" = '20250326005904_Add_Initial') THEN
    CREATE TABLE movies.inbox_message_consumers (
        inbox_message_id uuid NOT NULL,
        name character varying(500) NOT NULL,
        CONSTRAINT pk_inbox_message_consumers PRIMARY KEY (inbox_message_id, name)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM movies."__EFMigrationsHistory" WHERE "migration_id" = '20250326005904_Add_Initial') THEN
    CREATE TABLE movies.inbox_messages (
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
    IF NOT EXISTS(SELECT 1 FROM movies."__EFMigrationsHistory" WHERE "migration_id" = '20250326005904_Add_Initial') THEN
    CREATE TABLE movies.movies (
        id uuid NOT NULL,
        imdb_id text NOT NULL,
        title text NOT NULL,
        year text NOT NULL,
        plot text,
        image text NOT NULL,
        release_date text,
        youtube_trailer_url text,
        CONSTRAINT pk_movies PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM movies."__EFMigrationsHistory" WHERE "migration_id" = '20250326005904_Add_Initial') THEN
    CREATE TABLE movies.outbox_message_consumers (
        outbox_message_id uuid NOT NULL,
        name character varying(500) NOT NULL,
        CONSTRAINT pk_outbox_message_consumers PRIMARY KEY (outbox_message_id, name)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM movies."__EFMigrationsHistory" WHERE "migration_id" = '20250326005904_Add_Initial') THEN
    CREATE TABLE movies.outbox_messages (
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
    IF NOT EXISTS(SELECT 1 FROM movies."__EFMigrationsHistory" WHERE "migration_id" = '20250326005904_Add_Initial') THEN
    CREATE TABLE movies.people (
        id uuid NOT NULL,
        imdb_id text NOT NULL,
        name character varying(100) NOT NULL,
        CONSTRAINT pk_people PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM movies."__EFMigrationsHistory" WHERE "migration_id" = '20250326005904_Add_Initial') THEN
    CREATE TABLE movies.production_companies (
        id uuid NOT NULL,
        name text NOT NULL,
        imdb_id text NOT NULL,
        CONSTRAINT pk_production_companies PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM movies."__EFMigrationsHistory" WHERE "migration_id" = '20250326005904_Add_Initial') THEN
    CREATE TABLE movies.movie_genres (
        movie_id uuid NOT NULL,
        genre_id uuid NOT NULL,
        CONSTRAINT pk_movie_genres PRIMARY KEY (movie_id, genre_id),
        CONSTRAINT fk_movie_genres_genres_genre_id FOREIGN KEY (genre_id) REFERENCES movies.genres (id) ON DELETE CASCADE,
        CONSTRAINT fk_movie_genres_movies_movie_id FOREIGN KEY (movie_id) REFERENCES movies.movies (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM movies."__EFMigrationsHistory" WHERE "migration_id" = '20250326005904_Add_Initial') THEN
    CREATE TABLE movies.movie_actors (
        movie_id uuid NOT NULL,
        actor_id uuid NOT NULL,
        id uuid NOT NULL,
        CONSTRAINT pk_movie_actors PRIMARY KEY (movie_id, actor_id),
        CONSTRAINT fk_movie_actors_movies_movie_id FOREIGN KEY (movie_id) REFERENCES movies.movies (id) ON DELETE CASCADE,
        CONSTRAINT fk_movie_actors_people_actor_id FOREIGN KEY (actor_id) REFERENCES movies.people (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM movies."__EFMigrationsHistory" WHERE "migration_id" = '20250326005904_Add_Initial') THEN
    CREATE TABLE movies.movie_directors (
        movie_id uuid NOT NULL,
        director_id uuid NOT NULL,
        id uuid NOT NULL,
        CONSTRAINT pk_movie_directors PRIMARY KEY (movie_id, director_id),
        CONSTRAINT fk_movie_directors_movies_movie_id FOREIGN KEY (movie_id) REFERENCES movies.movies (id) ON DELETE CASCADE,
        CONSTRAINT fk_movie_directors_people_director_id FOREIGN KEY (director_id) REFERENCES movies.people (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM movies."__EFMigrationsHistory" WHERE "migration_id" = '20250326005904_Add_Initial') THEN
    CREATE TABLE movies.movie_producers (
        movie_id uuid NOT NULL,
        producer_id uuid NOT NULL,
        id uuid NOT NULL,
        CONSTRAINT pk_movie_producers PRIMARY KEY (movie_id, producer_id),
        CONSTRAINT fk_movie_producers_movies_movie_id FOREIGN KEY (movie_id) REFERENCES movies.movies (id) ON DELETE CASCADE,
        CONSTRAINT fk_movie_producers_people_producer_id FOREIGN KEY (producer_id) REFERENCES movies.people (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM movies."__EFMigrationsHistory" WHERE "migration_id" = '20250326005904_Add_Initial') THEN
    CREATE TABLE movies.movie_writers (
        movie_id uuid NOT NULL,
        writer_id uuid NOT NULL,
        id uuid NOT NULL,
        CONSTRAINT pk_movie_writers PRIMARY KEY (movie_id, writer_id),
        CONSTRAINT fk_movie_writers_movies_movie_id FOREIGN KEY (movie_id) REFERENCES movies.movies (id) ON DELETE CASCADE,
        CONSTRAINT fk_movie_writers_people_writer_id FOREIGN KEY (writer_id) REFERENCES movies.people (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM movies."__EFMigrationsHistory" WHERE "migration_id" = '20250326005904_Add_Initial') THEN
    CREATE TABLE movies.movie_production_companies (
        movie_id uuid NOT NULL,
        production_company_id uuid NOT NULL,
        CONSTRAINT pk_movie_production_companies PRIMARY KEY (movie_id, production_company_id),
        CONSTRAINT fk_movie_production_companies_movies_movie_id FOREIGN KEY (movie_id) REFERENCES movies.movies (id) ON DELETE CASCADE,
        CONSTRAINT fk_movie_production_companies_production_companies_production_ FOREIGN KEY (production_company_id) REFERENCES movies.production_companies (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM movies."__EFMigrationsHistory" WHERE "migration_id" = '20250326005904_Add_Initial') THEN
    CREATE INDEX ix_movie_actors_actor_id ON movies.movie_actors (actor_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM movies."__EFMigrationsHistory" WHERE "migration_id" = '20250326005904_Add_Initial') THEN
    CREATE INDEX ix_movie_directors_director_id ON movies.movie_directors (director_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM movies."__EFMigrationsHistory" WHERE "migration_id" = '20250326005904_Add_Initial') THEN
    CREATE INDEX ix_movie_genres_genre_id ON movies.movie_genres (genre_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM movies."__EFMigrationsHistory" WHERE "migration_id" = '20250326005904_Add_Initial') THEN
    CREATE INDEX ix_movie_producers_producer_id ON movies.movie_producers (producer_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM movies."__EFMigrationsHistory" WHERE "migration_id" = '20250326005904_Add_Initial') THEN
    CREATE INDEX ix_movie_production_companies_production_company_id ON movies.movie_production_companies (production_company_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM movies."__EFMigrationsHistory" WHERE "migration_id" = '20250326005904_Add_Initial') THEN
    CREATE INDEX ix_movie_writers_writer_id ON movies.movie_writers (writer_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM movies."__EFMigrationsHistory" WHERE "migration_id" = '20250326005904_Add_Initial') THEN
    CREATE INDEX ix_movies_imdb_id ON movies.movies (imdb_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM movies."__EFMigrationsHistory" WHERE "migration_id" = '20250326005904_Add_Initial') THEN
    CREATE INDEX ix_production_companies_imdb_id ON movies.production_companies (imdb_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM movies."__EFMigrationsHistory" WHERE "migration_id" = '20250326005904_Add_Initial') THEN
    INSERT INTO movies."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20250326005904_Add_Initial', '10.0.3');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM movies."__EFMigrationsHistory" WHERE "migration_id" = '20260308202714_Add_Tmdb_Id_Null') THEN
    ALTER TABLE movies.production_companies ADD tmdb_id integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM movies."__EFMigrationsHistory" WHERE "migration_id" = '20260308202714_Add_Tmdb_Id_Null') THEN
    ALTER TABLE movies.people ADD tmdb_id integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM movies."__EFMigrationsHistory" WHERE "migration_id" = '20260308202714_Add_Tmdb_Id_Null') THEN
    ALTER TABLE movies.movies ADD tmdb_id integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM movies."__EFMigrationsHistory" WHERE "migration_id" = '20260308202714_Add_Tmdb_Id_Null') THEN
    ALTER TABLE movies.genres ADD tmdb_id integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM movies."__EFMigrationsHistory" WHERE "migration_id" = '20260308202714_Add_Tmdb_Id_Null') THEN
    CREATE INDEX ix_people_tmdb_id ON movies.people (tmdb_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM movies."__EFMigrationsHistory" WHERE "migration_id" = '20260308202714_Add_Tmdb_Id_Null') THEN
    CREATE INDEX ix_movies_tmdb_id ON movies.movies (tmdb_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM movies."__EFMigrationsHistory" WHERE "migration_id" = '20260308202714_Add_Tmdb_Id_Null') THEN
    INSERT INTO movies."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20260308202714_Add_Tmdb_Id_Null', '10.0.3');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;
 
-- ============================================================
-- Update_Media migration — single consolidated block
-- Order: rename → alter columns → create media_* tables →
--        copy data → drop movie_* tables → indexes → public_id backfill
-- ============================================================
 
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM movies."__EFMigrationsHistory" WHERE "migration_id" = '20260322153034_Update_Media') THEN
 
    -- Step 1: Rename movies.movies → movies.media
    ALTER TABLE movies.movies RENAME TO media;
 
    -- Step 2: Alter columns on movies.media
    ALTER TABLE movies.media ALTER COLUMN tmdb_id DROP NOT NULL;
    ALTER TABLE movies.media ALTER COLUMN imdb_id DROP NOT NULL;
    ALTER TABLE movies.media ALTER COLUMN image DROP NOT NULL;
    ALTER TABLE movies.media ADD episode_number integer;
    ALTER TABLE movies.media ADD external_id text;
    ALTER TABLE movies.media ADD igdb_id integer;
    ALTER TABLE movies.media ADD media_type integer NOT NULL DEFAULT 0;
    ALTER TABLE movies.media ADD public_id character varying(17) NOT NULL DEFAULT '';
    ALTER TABLE movies.media ADD season_number integer;
    ALTER TABLE movies.media ADD tv_series_tmdb_id integer;
 
    -- Step 3: Create new media_* junction tables
    CREATE TABLE movies.media_actors (
        media_id uuid NOT NULL,
        actor_id uuid NOT NULL,
        id uuid NOT NULL,
        CONSTRAINT pk_media_actors PRIMARY KEY (media_id, actor_id),
        CONSTRAINT fk_media_actors_media_media_id FOREIGN KEY (media_id) REFERENCES movies.media (id) ON DELETE CASCADE,
        CONSTRAINT fk_media_actors_people_actor_id FOREIGN KEY (actor_id) REFERENCES movies.people (id) ON DELETE CASCADE
    );
 
    CREATE TABLE movies.media_directors (
        media_id uuid NOT NULL,
        director_id uuid NOT NULL,
        id uuid NOT NULL,
        CONSTRAINT pk_media_directors PRIMARY KEY (media_id, director_id),
        CONSTRAINT fk_media_directors_media_media_id FOREIGN KEY (media_id) REFERENCES movies.media (id) ON DELETE CASCADE,
        CONSTRAINT fk_media_directors_people_director_id FOREIGN KEY (director_id) REFERENCES movies.people (id) ON DELETE CASCADE
    );
 
    CREATE TABLE movies.media_genres (
        media_id uuid NOT NULL,
        genre_id uuid NOT NULL,
        CONSTRAINT pk_media_genres PRIMARY KEY (media_id, genre_id),
        CONSTRAINT fk_media_genres_genres_genre_id FOREIGN KEY (genre_id) REFERENCES movies.genres (id) ON DELETE CASCADE,
        CONSTRAINT fk_media_genres_media_media_id FOREIGN KEY (media_id) REFERENCES movies.media (id) ON DELETE CASCADE
    );
 
    CREATE TABLE movies.media_producers (
        media_id uuid NOT NULL,
        producer_id uuid NOT NULL,
        id uuid NOT NULL,
        CONSTRAINT pk_media_producers PRIMARY KEY (media_id, producer_id),
        CONSTRAINT fk_media_producers_media_media_id FOREIGN KEY (media_id) REFERENCES movies.media (id) ON DELETE CASCADE,
        CONSTRAINT fk_media_producers_people_producer_id FOREIGN KEY (producer_id) REFERENCES movies.people (id) ON DELETE CASCADE
    );
 
    CREATE TABLE movies.media_production_companies (
        media_id uuid NOT NULL,
        production_company_id uuid NOT NULL,
        CONSTRAINT pk_media_production_companies PRIMARY KEY (media_id, production_company_id),
        CONSTRAINT fk_media_production_companies_media_media_id FOREIGN KEY (media_id) REFERENCES movies.media (id) ON DELETE CASCADE,
        CONSTRAINT fk_media_production_companies_production_companies_production_ FOREIGN KEY (production_company_id) REFERENCES movies.production_companies (id) ON DELETE CASCADE
    );
 
    CREATE TABLE movies.media_writers (
        media_id uuid NOT NULL,
        writer_id uuid NOT NULL,
        id uuid NOT NULL,
        CONSTRAINT pk_media_writers PRIMARY KEY (media_id, writer_id),
        CONSTRAINT fk_media_writers_media_media_id FOREIGN KEY (media_id) REFERENCES movies.media (id) ON DELETE CASCADE,
        CONSTRAINT fk_media_writers_people_writer_id FOREIGN KEY (writer_id) REFERENCES movies.people (id) ON DELETE CASCADE
    );
 
    -- Step 4: Copy data from old movie_* tables to new media_* tables
    INSERT INTO movies.media_actors (media_id, actor_id, id)
    SELECT movie_id, actor_id, id FROM movies.movie_actors
    ON CONFLICT DO NOTHING;
 
    INSERT INTO movies.media_directors (media_id, director_id, id)
    SELECT movie_id, director_id, id FROM movies.movie_directors
    ON CONFLICT DO NOTHING;
 
    INSERT INTO movies.media_genres (media_id, genre_id)
    SELECT movie_id, genre_id FROM movies.movie_genres
    ON CONFLICT DO NOTHING;
 
    INSERT INTO movies.media_producers (media_id, producer_id, id)
    SELECT movie_id, producer_id, id FROM movies.movie_producers
    ON CONFLICT DO NOTHING;
 
    INSERT INTO movies.media_production_companies (media_id, production_company_id)
    SELECT movie_id, production_company_id FROM movies.movie_production_companies
    ON CONFLICT DO NOTHING;
 
    INSERT INTO movies.media_writers (media_id, writer_id, id)
    SELECT movie_id, writer_id, id FROM movies.movie_writers
    ON CONFLICT DO NOTHING;
 
    -- Step 5: Drop old movie_* tables
    DROP TABLE movies.movie_actors;
    DROP TABLE movies.movie_directors;
    DROP TABLE movies.movie_genres;
    DROP TABLE movies.movie_producers;
    DROP TABLE movies.movie_production_companies;
    DROP TABLE movies.movie_writers;
 
    -- Step 6: Create indexes on new media_* tables
    CREATE INDEX ix_media_igdb_id ON movies.media (igdb_id);
    CREATE UNIQUE INDEX ux_media_public_id ON movies.media (public_id) WHERE public_id <> '';
    CREATE INDEX ix_media_actors_actor_id ON movies.media_actors (actor_id);
    CREATE INDEX ix_media_directors_director_id ON movies.media_directors (director_id);
    CREATE INDEX ix_media_genres_genre_id ON movies.media_genres (genre_id);
    CREATE INDEX ix_media_producers_producer_id ON movies.media_producers (producer_id);
    CREATE INDEX ix_media_production_companies_production_company_id ON movies.media_production_companies (production_company_id);
    CREATE INDEX ix_media_writers_writer_id ON movies.media_writers (writer_id);
 
    -- Step 7: Backfill placeholder public_ids for existing rows.
    -- Real NanoIds are generated by PublicIdBackfillSeeder afterward.
    UPDATE movies.media
    SET public_id = 'm_' || LEFT(REPLACE(id::text, '-', ''), 15)
    WHERE public_id = '';
 
    INSERT INTO movies."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20260322153034_Update_Media', '10.0.3');
 
    END IF;
END $EF$;
COMMIT;