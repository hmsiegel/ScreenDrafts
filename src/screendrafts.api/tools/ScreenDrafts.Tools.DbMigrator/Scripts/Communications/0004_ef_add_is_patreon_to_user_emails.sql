ALTER TABLE communications.user_emails
    ADD COLUMN is_patreon boolean NOT NULL DEFAULT false;