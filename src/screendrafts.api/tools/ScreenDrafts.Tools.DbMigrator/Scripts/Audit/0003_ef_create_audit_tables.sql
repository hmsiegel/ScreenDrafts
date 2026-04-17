CREATE TABLE IF NOT EXISTS audit.http_audit_logs
(
    id                  uuid            NOT NULL,
    correlation_id      uuid            NOT NULL,
    occurred_on_utc     timestamptz     NOT NULL,
    actor_id            text,
    endpoint_name       text            NOT NULL,
    http_method         text            NOT NULL,
    route               text            NOT NULL,
    status_code         int,
    duration_ms         int,
    request_body        jsonb,
    response_body       jsonb,
    ip_address          text,
    CONSTRAINT pk_audit_logs PRIMARY KEY (id)
);

CREATE INDEX IF NOT EXISTS ix_http_audit_logs_actor_id       ON audit.http_audit_logs (actor_id);
CREATE INDEX IF NOT EXISTS ix_http_audit_logs_occurred_on    ON audit.http_audit_logs (occurred_on_utc DESC);
CREATE INDEX IF NOT EXISTS ix_http_audit_logs_status_code    ON audit.http_audit_logs (status_code);

CREATE TABLE IF NOT EXISTS audit.domain_event_audit_logs
(
    id                  uuid            NOT NULL,
    occurred_on_utc     timestamptz     NOT NULL,
    event_type          text            NOT NULL,
    source_module       text            NOT NULL,
    actor_id            text,
    entity_id           text,
    payload             jsonb           NOT NULL,
    CONSTRAINT pk_domain_event_audit_logs PRIMARY KEY (id)
);

CREATE INDEX IF NOT EXISTS ix_domain_event_occurred_on       ON audit.domain_event_audit_logs (occurred_on_utc DESC);
CREATE INDEX IF NOT EXISTS ix_domain_event_event_type        ON audit.domain_event_audit_logs (event_type);
CREATE INDEX IF NOT EXISTS ix_domain_event_source_module     ON audit.domain_event_audit_logs (source_module);
CREATE INDEX IF NOT EXISTS ix_domain_event_actor_id          ON audit.domain_event_audit_logs (actor_id);

CREATE TABLE IF NOT EXISTS audit.auth_audit_logs
(
    id                  uuid            NOT NULL,
    occurred_on_utc     timestamptz     NOT NULL,
    event_type          text            NOT NULL,
    user_id             text,
    client_id           text,
    ip_address          text,
    details             jsonb,
    CONSTRAINT pk_auth_audit_logs PRIMARY KEY (id)
);

CREATE INDEX IF NOT EXISTS ix_auth_audit_logs_user_id        ON audit.auth_audit_logs (user_id);
CREATE INDEX IF NOT EXISTS ix_auth_audit_logs_occurred_on    ON audit.auth_audit_logs (occurred_on_utc DESC);
CREATE INDEX IF NOT EXISTS ix_auth_audit_logs_event_type     ON audit.auth_audit_logs (event_type);