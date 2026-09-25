CREATE TABLE communications.email_deliveries (
    event_id      uuid                     NOT NULL,
    email_address text                     NOT NULL,
    sent_on_utc   timestamp with time zone NOT NULL,
    CONSTRAINT pk_email_deliveries PRIMARY KEY (event_id, email_address)
);