-- =============================================================================
-- Administration: Permissions, Roles, RolePermissions, UserRoles
-- Creates the access-control tables that are moving from the users schema.
-- No foreign key from user_roles.user_id to users.users — user_id is a plain
-- UUID; the Users module owns referential integrity on its side.
-- =============================================================================
 
CREATE TABLE administration.permissions
(
    code VARCHAR(100) NOT NULL,
    CONSTRAINT pk_permissions PRIMARY KEY (code)
);
 
CREATE TABLE administration.roles
(
    name VARCHAR(50) NOT NULL,
    CONSTRAINT pk_roles PRIMARY KEY (name)
);
 
CREATE TABLE administration.role_permissions
(
    permission_code VARCHAR(100) NOT NULL,
    role_name       VARCHAR(50)  NOT NULL,
    CONSTRAINT pk_role_permissions PRIMARY KEY (permission_code, role_name),
    CONSTRAINT fk_role_permissions_permission_code
        FOREIGN KEY (permission_code) REFERENCES administration.permissions (code)
        ON DELETE CASCADE,
    CONSTRAINT fk_role_permissions_role_name
        FOREIGN KEY (role_name) REFERENCES administration.roles (name)
        ON DELETE CASCADE
);
 
CREATE INDEX ix_role_permissions_role_name
    ON administration.role_permissions (role_name);
 
CREATE TABLE administration.user_roles
(
    user_id   UUID        NOT NULL,
    role_name VARCHAR(50) NOT NULL,
    CONSTRAINT pk_user_roles PRIMARY KEY (user_id, role_name),
    CONSTRAINT fk_user_roles_role_name
        FOREIGN KEY (role_name) REFERENCES administration.roles (name)
        ON DELETE CASCADE
);
 
CREATE INDEX ix_user_roles_user_id
    ON administration.user_roles (user_id);