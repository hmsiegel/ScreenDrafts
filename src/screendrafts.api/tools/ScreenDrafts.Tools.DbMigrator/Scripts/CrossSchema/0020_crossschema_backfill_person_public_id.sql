-- Fills reporting.pick_credit_facts.drafter_person_public_id from drafts.people.
-- The site's drafter pages (/drafters/{id}) are keyed by the person's public id, not the drafter's, so the
-- Record Book and the custom query return the person id for every drafter holder.
-- Runs after the Reporting migration that adds the column (default empty string) and after 0020, which creates
-- the credit rows. Idempotent. No explicit transaction: the DbMigrator runner wraps the script.

UPDATE reporting.pick_credit_facts c
SET drafter_person_public_id = pe.public_id
FROM drafts.drafters dr
JOIN drafts.people pe ON pe.id = dr.person_id
WHERE dr.id = c.drafter_id_value
  AND c.drafter_person_public_id IS DISTINCT FROM pe.public_id;