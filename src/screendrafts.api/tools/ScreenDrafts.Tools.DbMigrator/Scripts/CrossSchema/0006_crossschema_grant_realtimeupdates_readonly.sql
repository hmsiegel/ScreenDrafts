-- Grants real_time_updates_user SELECT-only access to drafts.* and
-- reporting.* schemas. RealTimeUpdates has no domain tables of its own and
-- performs no writes outside its own outbox/inbox — it exists purely to
-- read current state and broadcast it over SignalR. That makes it a
-- sanctioned exception to the "no cross-schema SQL" rule, not a violation
-- of it: the rule protects write ownership and schema coupling between
-- modules that own domain logic, neither of which applies here. The grant
-- is intentionally SELECT-only — RealTimeUpdates must never be able to
-- write into another module's schema.
--
-- ALTER DEFAULT PRIVILEGES covers tables created AFTER this script runs,
-- so new drafts/reporting tables don't silently need a follow-up grant.

GRANT USAGE ON SCHEMA drafts TO real_time_updates_user;
GRANT SELECT ON ALL TABLES IN SCHEMA drafts TO real_time_updates_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA drafts
  GRANT SELECT ON TABLES TO real_time_updates_user;

GRANT USAGE ON SCHEMA reporting TO real_time_updates_user;
GRANT SELECT ON ALL TABLES IN SCHEMA reporting TO real_time_updates_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA reporting
  GRANT SELECT ON TABLES TO real_time_updates_user;