-- Minimal user email read model seeded from UserRegisteredIntegrationEvent.
-- ON CONFLICT DO UPDATE keeps the address current if the user ever re-registers.
CREATE TABLE IF NOT EXISTS communications.user_emails
(
    user_id       uuid         NOT NULL,
    email_address varchar(320) NOT NULL,
    full_name     varchar(500) NOT NULL,

    CONSTRAINT pk_user_emails PRIMARY KEY (user_id)
);