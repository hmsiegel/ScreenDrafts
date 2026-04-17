CREATE TABLE IF NOT EXISTS audit.auth_event_watermark
(
    id               int         NOT NULL DEFAULT 1,
    last_event_time  timestamptz NOT NULL,
    CONSTRAINT pk_auth_event_watermark PRIMARY KEY (id),
    CONSTRAINT chk_single_row CHECK (id = 1)
);