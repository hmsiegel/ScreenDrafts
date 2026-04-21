DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'users') THEN
        CREATE SCHEMA users;
    END IF;
END $EF$;
CREATE TABLE IF NOT EXISTS users."__EFMigrationsHistory" (
    migration_id character varying(150) NOT NULL,
    product_version character varying(32) NOT NULL,
    CONSTRAINT pk___ef_migrations_history PRIMARY KEY (migration_id)
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20250131115711_Add_Initial') THEN
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'users') THEN
            CREATE SCHEMA users;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20250131115711_Add_Initial') THEN
    CREATE TABLE users.permissions (
        code character varying(100) NOT NULL,
        CONSTRAINT pk_permissions PRIMARY KEY (code)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20250131115711_Add_Initial') THEN
    CREATE TABLE users.roles (
        name character varying(50) NOT NULL,
        CONSTRAINT pk_roles PRIMARY KEY (name)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20250131115711_Add_Initial') THEN
    CREATE TABLE users.users (
        id uuid NOT NULL,
        email character varying(255) NOT NULL,
        first_name character varying(50) NOT NULL,
        middle_name text,
        last_name character varying(50) NOT NULL,
        identity_id text NOT NULL,
        CONSTRAINT pk_users PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20250131115711_Add_Initial') THEN
    CREATE TABLE users.role_permissions (
        permission_code character varying(100) NOT NULL,
        role_name character varying(50) NOT NULL,
        CONSTRAINT pk_role_permissions PRIMARY KEY (permission_code, role_name),
        CONSTRAINT fk_role_permissions_permissions_permission_code FOREIGN KEY (permission_code) REFERENCES users.permissions (code) ON DELETE CASCADE,
        CONSTRAINT fk_role_permissions_roles_role_name FOREIGN KEY (role_name) REFERENCES users.roles (name) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20250131115711_Add_Initial') THEN
    CREATE TABLE users.user_roles (
        role_name character varying(50) NOT NULL,
        user_id uuid NOT NULL,
        CONSTRAINT pk_user_roles PRIMARY KEY (role_name, user_id),
        CONSTRAINT fk_user_roles_roles_roles_name FOREIGN KEY (role_name) REFERENCES users.roles (name) ON DELETE CASCADE,
        CONSTRAINT fk_user_roles_users_user_id FOREIGN KEY (user_id) REFERENCES users.users (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20250131115711_Add_Initial') THEN
    INSERT INTO users.permissions (code)
    VALUES ('actors:search');
    INSERT INTO users.permissions (code)
    VALUES ('crew:search');
    INSERT INTO users.permissions (code)
    VALUES ('drafters:add');
    INSERT INTO users.permissions (code)
    VALUES ('drafters:read');
    INSERT INTO users.permissions (code)
    VALUES ('drafters:remove');
    INSERT INTO users.permissions (code)
    VALUES ('drafters:update');
    INSERT INTO users.permissions (code)
    VALUES ('drafts:create');
    INSERT INTO users.permissions (code)
    VALUES ('drafts:read');
    INSERT INTO users.permissions (code)
    VALUES ('drafts:search');
    INSERT INTO users.permissions (code)
    VALUES ('drafts:update');
    INSERT INTO users.permissions (code)
    VALUES ('genres:search');
    INSERT INTO users.permissions (code)
    VALUES ('hosts:add');
    INSERT INTO users.permissions (code)
    VALUES ('hosts:read');
    INSERT INTO users.permissions (code)
    VALUES ('hosts:remove');
    INSERT INTO users.permissions (code)
    VALUES ('hosts:update');
    INSERT INTO users.permissions (code)
    VALUES ('movies:search');
    INSERT INTO users.permissions (code)
    VALUES ('permissions:read');
    INSERT INTO users.permissions (code)
    VALUES ('permissions:update');
    INSERT INTO users.permissions (code)
    VALUES ('picks:add');
    INSERT INTO users.permissions (code)
    VALUES ('picks:veto');
    INSERT INTO users.permissions (code)
    VALUES ('picks:veto-override');
    INSERT INTO users.permissions (code)
    VALUES ('roles:read');
    INSERT INTO users.permissions (code)
    VALUES ('roles:update');
    INSERT INTO users.permissions (code)
    VALUES ('studios:search');
    INSERT INTO users.permissions (code)
    VALUES ('users:read');
    INSERT INTO users.permissions (code)
    VALUES ('users:update');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20250131115711_Add_Initial') THEN
    INSERT INTO users.roles (name)
    VALUES ('Administrator');
    INSERT INTO users.roles (name)
    VALUES ('Drafter');
    INSERT INTO users.roles (name)
    VALUES ('Guest');
    INSERT INTO users.roles (name)
    VALUES ('Host');
    INSERT INTO users.roles (name)
    VALUES ('SuperAdministrator');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20250131115711_Add_Initial') THEN
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('actors:search', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('actors:search', 'Drafter');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('actors:search', 'Guest');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('actors:search', 'Host');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('actors:search', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('crew:search', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('crew:search', 'Drafter');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('crew:search', 'Guest');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('crew:search', 'Host');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('crew:search', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('drafters:add', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('drafters:add', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('drafters:read', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('drafters:read', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('drafters:remove', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('drafters:remove', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('drafters:update', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('drafters:update', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('drafts:create', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('drafts:create', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('drafts:read', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('drafts:read', 'Host');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('drafts:read', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('drafts:search', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('drafts:search', 'Drafter');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('drafts:search', 'Guest');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('drafts:search', 'Host');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('drafts:search', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('drafts:update', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('drafts:update', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('genres:search', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('genres:search', 'Drafter');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('genres:search', 'Guest');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('genres:search', 'Host');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('genres:search', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('hosts:add', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('hosts:add', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('hosts:read', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('hosts:read', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('hosts:remove', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('hosts:remove', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('hosts:update', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('hosts:update', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('movies:search', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('movies:search', 'Drafter');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('movies:search', 'Guest');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('movies:search', 'Host');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('movies:search', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('permissions:read', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('permissions:update', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('picks:add', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('picks:add', 'Drafter');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('picks:add', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('picks:veto', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('picks:veto', 'Drafter');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('picks:veto', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('picks:veto-override', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('picks:veto-override', 'Drafter');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('picks:veto-override', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('roles:read', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('roles:update', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('studios:search', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('studios:search', 'Drafter');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('studios:search', 'Guest');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('studios:search', 'Host');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('studios:search', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('users:read', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('users:read', 'Drafter');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('users:read', 'Guest');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('users:read', 'Host');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('users:read', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('users:update', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('users:update', 'Drafter');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('users:update', 'Guest');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('users:update', 'Host');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('users:update', 'SuperAdministrator');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20250131115711_Add_Initial') THEN
    CREATE INDEX ix_role_permissions_role_name ON users.role_permissions (role_name);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20250131115711_Add_Initial') THEN
    CREATE INDEX ix_user_roles_user_id ON users.user_roles (user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20250131115711_Add_Initial') THEN
    CREATE UNIQUE INDEX ix_users_email ON users.users (email);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20250131115711_Add_Initial') THEN
    CREATE UNIQUE INDEX ix_users_identity_id ON users.users (identity_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20250131115711_Add_Initial') THEN
    INSERT INTO users."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20250131115711_Add_Initial', '10.0.3');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20250201142920_Add_Outbox') THEN
    CREATE TABLE users.outbox_messages (
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
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20250201142920_Add_Outbox') THEN
    INSERT INTO users."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20250201142920_Add_Outbox', '10.0.3');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20250203034308_Add_Inbox') THEN
    CREATE TABLE users.inbox_message_consumers (
        inbox_message_id uuid NOT NULL,
        name character varying(500) NOT NULL,
        CONSTRAINT pk_inbox_message_consumers PRIMARY KEY (inbox_message_id, name)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20250203034308_Add_Inbox') THEN
    CREATE TABLE users.inbox_messages (
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
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20250203034308_Add_Inbox') THEN
    CREATE TABLE users.outbox_message_consumers (
        outbox_message_id uuid NOT NULL,
        name character varying(500) NOT NULL,
        CONSTRAINT pk_outbox_message_consumers PRIMARY KEY (outbox_message_id, name)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20250203034308_Add_Inbox') THEN
    INSERT INTO users."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20250203034308_Add_Inbox', '10.0.3');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20250203124008_Update_Permissions') THEN
    INSERT INTO users.permissions (code)
    VALUES ('drafters:create');
    INSERT INTO users.permissions (code)
    VALUES ('hosts:create');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20250203124008_Update_Permissions') THEN
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('drafters:create', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('hosts:create', 'Administrator');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20250203124008_Update_Permissions') THEN
    INSERT INTO users."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20250203124008_Update_Permissions', '10.0.3');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20250214203724_AddPatreonRole') THEN
    INSERT INTO users.permissions (code)
    VALUES ('patreon:search');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20250214203724_AddPatreonRole') THEN
    INSERT INTO users.roles (name)
    VALUES ('Patreon');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20250214203724_AddPatreonRole') THEN
    INSERT INTO users.role_permissions (role_name, permission_code)
    VALUES ('Patreon', 'patreon:search');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20250214203724_AddPatreonRole') THEN
    INSERT INTO users."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20250214203724_AddPatreonRole', '10.0.3');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20250718203645_Update_User_Permissions') THEN
    INSERT INTO users.permissions (code)
    VALUES ('people:create');
    INSERT INTO users.permissions (code)
    VALUES ('people:read');
    INSERT INTO users.permissions (code)
    VALUES ('people:search');
    INSERT INTO users.permissions (code)
    VALUES ('people:update');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20250718203645_Update_User_Permissions') THEN
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('people:create', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('people:create', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('people:read', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('people:read', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('people:search', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('people:search', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('people:update', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('people:update', 'SuperAdministrator');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20250718203645_Update_User_Permissions') THEN
    INSERT INTO users."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20250718203645_Update_User_Permissions', '10.0.3');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20250722165044_Add_UserProfile_SocialLinks') THEN
    ALTER TABLE users.users ADD instagram_handle character varying(100);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20250722165044_Add_UserProfile_SocialLinks') THEN
    ALTER TABLE users.users ADD letterboxd_handle character varying(100);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20250722165044_Add_UserProfile_SocialLinks') THEN
    ALTER TABLE users.users ADD profile_picture_url character varying(2048);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20250722165044_Add_UserProfile_SocialLinks') THEN
    ALTER TABLE users.users ADD twitter_handle character varying(100);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20250722165044_Add_UserProfile_SocialLinks') THEN
    INSERT INTO users."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20250722165044_Add_UserProfile_SocialLinks', '10.0.3');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20250722173352_Add_BlueskyHandle') THEN
    ALTER TABLE users.users ADD bluesky_handle character varying(100);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20250722173352_Add_BlueskyHandle') THEN
    INSERT INTO users."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20250722173352_Add_BlueskyHandle', '10.0.3');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20250724013116_Add_PersonId') THEN
    ALTER TABLE users.users ADD person_id uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20250724013116_Add_PersonId') THEN
    CREATE UNIQUE INDEX ix_users_person_id ON users.users (person_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20250724013116_Add_PersonId') THEN
    INSERT INTO users."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20250724013116_Add_PersonId', '10.0.3');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20250725140250_Update_ProfilePicture') THEN
    ALTER TABLE users.users RENAME COLUMN profile_picture_url TO profile_picture_path;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20250725140250_Update_ProfilePicture') THEN
    INSERT INTO users."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20250725140250_Update_ProfilePicture', '10.0.3');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20250828110814_Update_Permissions_with_Categories') THEN
    INSERT INTO users.permissions (code)
    VALUES ('categories:create');
    INSERT INTO users.permissions (code)
    VALUES ('categories:delete');
    INSERT INTO users.permissions (code)
    VALUES ('categories:read');
    INSERT INTO users.permissions (code)
    VALUES ('categories:search');
    INSERT INTO users.permissions (code)
    VALUES ('categories:update');
    INSERT INTO users.permissions (code)
    VALUES ('movies:create');
    INSERT INTO users.permissions (code)
    VALUES ('movies:read');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20250828110814_Update_Permissions_with_Categories') THEN
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('people:search', 'Drafter');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('people:search', 'Guest');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('people:search', 'Host');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('categories:create', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('categories:create', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('categories:delete', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('categories:delete', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('categories:read', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('categories:read', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('categories:search', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('categories:search', 'Drafter');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('categories:search', 'Guest');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('categories:search', 'Host');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('categories:search', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('categories:update', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('categories:update', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('movies:create', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('movies:create', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('movies:read', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('movies:read', 'SuperAdministrator');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20250828110814_Update_Permissions_with_Categories') THEN
    INSERT INTO users."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20250828110814_Update_Permissions_with_Categories', '10.0.3');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20260211123051_Add_PublicId_And_PersonPublicId') THEN
    ALTER TABLE users.users ADD person_public_id text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20260211123051_Add_PublicId_And_PersonPublicId') THEN
    ALTER TABLE users.users ADD public_id text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20260211123051_Add_PublicId_And_PersonPublicId') THEN
    CREATE UNIQUE INDEX IF NOT EXISTS ix_users_person_public_id
    on users.users (person_public_id)
    WHERE person_public_id IS NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20260211123051_Add_PublicId_And_PersonPublicId') THEN
    INSERT INTO users."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20260211123051_Add_PublicId_And_PersonPublicId', '10.0.3');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20260211134105_Update_User_Config') THEN
    DO $$
    BEGIN
        IF EXISTS (SELECT 1 FROM users.users WHERE public_id IS NULL OR public_id = '') THEN
            RAISE EXCEPTION 'users.users.public_id has null/empty rows; backfill before enforcing';
        END IF;
    END $$;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20260211134105_Update_User_Config') THEN
    ALTER TABLE users.users ALTER COLUMN public_id SET NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20260211134105_Update_User_Config') THEN
    CREATE UNIQUE INDEX ix_users_public_id ON users.users (public_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20260211134105_Update_User_Config') THEN
    DROP INDEX users.ix_users_person_id;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20260211134105_Update_User_Config') THEN
    DROP INDEX users.ix_users_person_public_id;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20260211134105_Update_User_Config') THEN
    ALTER TABLE users.users ALTER COLUMN identity_id TYPE character varying(255);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20260211134105_Update_User_Config') THEN
    CREATE UNIQUE INDEX ix_users_person_id ON users.users (person_id) WHERE "person_id" IS NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20260211134105_Update_User_Config') THEN
    CREATE UNIQUE INDEX ix_users_person_public_id ON users.users (person_public_id) WHERE "person_public_id" IS NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20260211134105_Update_User_Config') THEN
    INSERT INTO users."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20260211134105_Update_User_Config', '10.0.3');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20260316203821_Reconcile_User_Permissions') THEN
    INSERT INTO users.permissions (code)
    VALUES ('campaigns:create');
    INSERT INTO users.permissions (code)
    VALUES ('campaigns:delete');
    INSERT INTO users.permissions (code)
    VALUES ('campaigns:list');
    INSERT INTO users.permissions (code)
    VALUES ('campaigns:read');
    INSERT INTO users.permissions (code)
    VALUES ('campaigns:restore');
    INSERT INTO users.permissions (code)
    VALUES ('campaigns:update');
    INSERT INTO users.permissions (code)
    VALUES ('candidate-lists:create');
    INSERT INTO users.permissions (code)
    VALUES ('candidate-lists:delete');
    INSERT INTO users.permissions (code)
    VALUES ('candidate-lists:list');
    INSERT INTO users.permissions (code)
    VALUES ('candidate-lists:read');
    INSERT INTO users.permissions (code)
    VALUES ('candidate-lists:update');
    INSERT INTO users.permissions (code)
    VALUES ('draft-boards:create');
    INSERT INTO users.permissions (code)
    VALUES ('draft-boards:delete');
    INSERT INTO users.permissions (code)
    VALUES ('draft-boards:list');
    INSERT INTO users.permissions (code)
    VALUES ('draft-boards:read');
    INSERT INTO users.permissions (code)
    VALUES ('draft-boards:update');
    INSERT INTO users.permissions (code)
    VALUES ('draft-parts:create');
    INSERT INTO users.permissions (code)
    VALUES ('draft-parts:list');
    INSERT INTO users.permissions (code)
    VALUES ('draft-parts:read');
    INSERT INTO users.permissions (code)
    VALUES ('draft-parts:set-status');
    INSERT INTO users.permissions (code)
    VALUES ('draft-parts:update');
    INSERT INTO users.permissions (code)
    VALUES ('draft-pools:create');
    INSERT INTO users.permissions (code)
    VALUES ('draft-pools:read');
    INSERT INTO users.permissions (code)
    VALUES ('draft-pools:update');
    INSERT INTO users.permissions (code)
    VALUES ('drafter-teams:create');
    INSERT INTO users.permissions (code)
    VALUES ('drafter-teams:list');
    INSERT INTO users.permissions (code)
    VALUES ('drafter-teams:members');
    INSERT INTO users.permissions (code)
    VALUES ('drafter-teams:read');
    INSERT INTO users.permissions (code)
    VALUES ('drafter-teams:update');
    INSERT INTO users.permissions (code)
    VALUES ('drafters:list');
    INSERT INTO users.permissions (code)
    VALUES ('drafts:delete');
    INSERT INTO users.permissions (code)
    VALUES ('drafts:list');
    INSERT INTO users.permissions (code)
    VALUES ('drafts:read-patreon');
    INSERT INTO users.permissions (code)
    VALUES ('game-boards:create');
    INSERT INTO users.permissions (code)
    VALUES ('game-boards:update');
    INSERT INTO users.permissions (code)
    VALUES ('hosts:list');
    INSERT INTO users.permissions (code)
    VALUES ('people:list');
    INSERT INTO users.permissions (code)
    VALUES ('picks:commissioner-override');
    INSERT INTO users.permissions (code)
    VALUES ('picks:create');
    INSERT INTO users.permissions (code)
    VALUES ('picks:undo');
    INSERT INTO users.permissions (code)
    VALUES ('picks:update');
    INSERT INTO users.permissions (code)
    VALUES ('series:create');
    INSERT INTO users.permissions (code)
    VALUES ('series:delete');
    INSERT INTO users.permissions (code)
    VALUES ('series:list');
    INSERT INTO users.permissions (code)
    VALUES ('series:read');
    INSERT INTO users.permissions (code)
    VALUES ('series:update');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20260316203821_Reconcile_User_Permissions') THEN
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('drafters:create', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('drafters:read', 'Drafter');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('drafters:read', 'Guest');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('drafters:read', 'Host');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('drafts:read', 'Drafter');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('hosts:create', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('hosts:read', 'Drafter');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('hosts:read', 'Guest');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('hosts:read', 'Host');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('people:read', 'Drafter');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('people:read', 'Host');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('campaigns:create', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('campaigns:create', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('campaigns:delete', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('campaigns:delete', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('campaigns:list', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('campaigns:list', 'Host');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('campaigns:list', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('campaigns:read', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('campaigns:read', 'Host');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('campaigns:read', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('campaigns:restore', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('campaigns:restore', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('campaigns:update', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('campaigns:update', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('candidate-lists:create', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('candidate-lists:create', 'Drafter');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('candidate-lists:create', 'Host');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('candidate-lists:create', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('candidate-lists:delete', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('candidate-lists:delete', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('candidate-lists:list', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('candidate-lists:list', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('candidate-lists:read', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('candidate-lists:read', 'Drafter');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('candidate-lists:read', 'Host');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('candidate-lists:read', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('candidate-lists:update', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('candidate-lists:update', 'Drafter');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('candidate-lists:update', 'Host');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('candidate-lists:update', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('draft-boards:create', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('draft-boards:create', 'Drafter');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('draft-boards:create', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('draft-boards:delete', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('draft-boards:delete', 'Drafter');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('draft-boards:delete', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('draft-boards:list', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('draft-boards:list', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('draft-boards:read', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('draft-boards:read', 'Drafter');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('draft-boards:read', 'Host');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('draft-boards:read', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('draft-boards:update', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('draft-boards:update', 'Drafter');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('draft-boards:update', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('draft-parts:create', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('draft-parts:create', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('draft-parts:list', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('draft-parts:list', 'Drafter');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('draft-parts:list', 'Guest');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('draft-parts:list', 'Host');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('draft-parts:list', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('draft-parts:read', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('draft-parts:read', 'Drafter');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('draft-parts:read', 'Host');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('draft-parts:read', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('draft-parts:set-status', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('draft-parts:set-status', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('draft-parts:update', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('draft-parts:update', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('draft-pools:create', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('draft-pools:create', 'Host');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('draft-pools:create', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('draft-pools:read', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('draft-pools:read', 'Drafter');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('draft-pools:read', 'Host');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('draft-pools:read', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('draft-pools:update', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('draft-pools:update', 'Host');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('draft-pools:update', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('drafter-teams:create', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('drafter-teams:create', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('drafter-teams:list', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('drafter-teams:list', 'Guest');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('drafter-teams:list', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('drafter-teams:members', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('drafter-teams:members', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('drafter-teams:read', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('drafter-teams:read', 'Guest');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('drafter-teams:read', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('drafter-teams:update', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('drafter-teams:update', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('drafters:list', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('drafters:list', 'Drafter');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('drafters:list', 'Guest');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('drafters:list', 'Host');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('drafters:list', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('drafts:delete', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('drafts:delete', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('drafts:list', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('drafts:list', 'Drafter');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('drafts:list', 'Guest');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('drafts:list', 'Host');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('drafts:list', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('drafts:read-patreon', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('drafts:read-patreon', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('game-boards:create', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('game-boards:create', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('game-boards:update', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('game-boards:update', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('hosts:list', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('hosts:list', 'Guest');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('hosts:list', 'Host');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('hosts:list', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('people:list', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('people:list', 'Host');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('people:list', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('picks:commissioner-override', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('picks:commissioner-override', 'Host');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('picks:commissioner-override', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('picks:create', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('picks:create', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('picks:undo', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('picks:undo', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('picks:update', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('picks:update', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('series:create', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('series:create', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('series:delete', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('series:delete', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('series:list', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('series:list', 'Drafter');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('series:list', 'Host');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('series:list', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('series:read', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('series:read', 'Drafter');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('series:read', 'Host');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('series:read', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('series:update', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('series:update', 'SuperAdministrator');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20260316203821_Reconcile_User_Permissions') THEN
    INSERT INTO users."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20260316203821_Reconcile_User_Permissions', '10.0.3');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20260406105600_Add_SpeedDrafts') THEN
    INSERT INTO users.permissions (code)
    VALUES ('sub-drafts:create');
    INSERT INTO users.permissions (code)
    VALUES ('sub-drafts:read');
    INSERT INTO users.permissions (code)
    VALUES ('sub-drafts:update');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20260406105600_Add_SpeedDrafts') THEN
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('sub-drafts:create', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('sub-drafts:create', 'Host');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('sub-drafts:create', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('sub-drafts:read', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('sub-drafts:read', 'Drafter');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('sub-drafts:read', 'Guest');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('sub-drafts:read', 'Host');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('sub-drafts:read', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('sub-drafts:update', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('sub-drafts:update', 'Host');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('sub-drafts:update', 'SuperAdministrator');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20260406105600_Add_SpeedDrafts') THEN
    INSERT INTO users."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20260406105600_Add_SpeedDrafts', '10.0.3');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20260409161917_Add_Predictions') THEN
    INSERT INTO users.permissions (code)
    VALUES ('predictions:manage');
    INSERT INTO users.permissions (code)
    VALUES ('predictions:read');
    INSERT INTO users.permissions (code)
    VALUES ('predictions:submit');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20260409161917_Add_Predictions') THEN
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('sub-drafts:create', 'Drafter');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('predictions:manage', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('predictions:manage', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('predictions:read', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('predictions:read', 'Drafter');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('predictions:read', 'Guest');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('predictions:read', 'Host');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('predictions:read', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('predictions:submit', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('predictions:submit', 'Host');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('predictions:submit', 'SuperAdministrator');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20260409161917_Add_Predictions') THEN
    INSERT INTO users."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20260409161917_Add_Predictions', '10.0.3');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20260416181523_Add_Audit_Permissions') THEN
    INSERT INTO users.permissions (code)
    VALUES ('audit:export');
    INSERT INTO users.permissions (code)
    VALUES ('audit:read');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20260416181523_Add_Audit_Permissions') THEN
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('audit:export', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('audit:export', 'SuperAdministrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('audit:read', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('audit:read', 'SuperAdministrator');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20260416181523_Add_Audit_Permissions') THEN
    INSERT INTO users."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20260416181523_Add_Audit_Permissions', '10.0.3');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20260419194245_Add_Admin_Permissions') THEN
    INSERT INTO users.permissions (code)
    VALUES ('admin:view-deleted');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20260419194245_Add_Admin_Permissions') THEN
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('admin:view-deleted', 'Administrator');
    INSERT INTO users.role_permissions (permission_code, role_name)
    VALUES ('admin:view-deleted', 'SuperAdministrator');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20260419194245_Add_Admin_Permissions') THEN
    INSERT INTO users."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20260419194245_Add_Admin_Permissions', '10.0.3');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20260420175252_Drop_Users_Access_Controls') THEN
    DROP TABLE users.role_permissions;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20260420175252_Drop_Users_Access_Controls') THEN
    DROP TABLE users.user_roles;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20260420175252_Drop_Users_Access_Controls') THEN
    DROP TABLE users.permissions;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20260420175252_Drop_Users_Access_Controls') THEN
    DROP TABLE users.roles;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM users."__EFMigrationsHistory" WHERE "migration_id" = '20260420175252_Drop_Users_Access_Controls') THEN
    INSERT INTO users."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20260420175252_Drop_Users_Access_Controls', '10.0.3');
    END IF;
END $EF$;
COMMIT;

