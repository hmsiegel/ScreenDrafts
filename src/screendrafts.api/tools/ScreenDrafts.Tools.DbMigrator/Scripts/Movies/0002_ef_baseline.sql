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

